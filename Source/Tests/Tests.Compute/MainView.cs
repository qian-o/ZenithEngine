using System.Numerics;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using Tests.Core;

namespace Tests.Compute;

internal sealed unsafe class MainView : View
{
    private struct DispatchSize
    {
        public uint X;

        public uint Y;
    }

    private readonly GraphicsDevice _device;
    private readonly ImGuiController _imGuiController;

    private readonly DeviceBuffer _buffer;
    private readonly ResourceLayout _resourceLayout;
    private readonly ResourceSet _resourceSet;
    private readonly Shader _shader;
    private readonly Pipeline _pipeline;
    private readonly CommandList _commandList;

    private Texture? _outputTexture;
    private TextureView? _outputTextureView;

    public MainView(GraphicsDevice device, ImGuiController imGuiController) : base("Compute View")
    {
        string hlsl = File.ReadAllText("Assets/Shaders/SDF.hlsl");

        _device = device;
        _imGuiController = imGuiController;

        _buffer = device.Factory.CreateBuffer(new BufferDescription((uint)sizeof(DispatchSize), BufferUsage.UniformBuffer));

        _resourceLayout = device.Factory.CreateResourceLayout(new ResourceLayoutDescription(new ResourceLayoutElementDescription("dispatchSize", ResourceKind.UniformBuffer, ShaderStages.Compute),
                                                                                            new ResourceLayoutElementDescription("outputTexture", ResourceKind.StorageImage, ShaderStages.Compute)));

        _resourceSet = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout, _buffer));

        _shader = device.Factory.CompileHlslToSpirv(new ShaderDescription(ShaderStages.Compute, Encoding.UTF8.GetBytes(hlsl), "main")).First();

        _pipeline = device.Factory.CreateComputePipeline(new ComputePipelineDescription(_shader, [_resourceLayout]));

        _commandList = device.Factory.CreateComputeCommandList();
    }

    protected override void OnUpdate(UpdateEventArgs e)
    {
    }

    protected override void OnRender(RenderEventArgs e)
    {
        if (_outputTexture != null)
        {
            _commandList.Begin();

            _commandList.SetPipeline(_pipeline);
            _commandList.SetResourceSet(0, _resourceSet);

            _commandList.Dispatch(_outputTexture.Width, _outputTexture.Height, 1);

            _commandList.End();

            _device.SubmitCommands(_commandList);

            ImGui.Image(_imGuiController.GetBinding(_device.Factory, _outputTexture), new Vector2(_outputTexture.Width, _outputTexture.Height));
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        DispatchSize dispatchSize = new()
        {
            X = e.Width,
            Y = e.Height
        };

        _device.UpdateBuffer(_buffer, 0, in dispatchSize);

        _outputTextureView?.Dispose();

        if (_outputTexture != null)
        {
            _imGuiController.RemoveBinding(_imGuiController.GetBinding(_device.Factory, _outputTexture));

            _outputTexture.Dispose();
        }

        _outputTexture = _device.Factory.CreateTexture(TextureDescription.Texture2D(e.Width,
                                                                                    e.Height,
                                                                                    1,
                                                                                    PixelFormat.R8G8B8A8UNorm,
                                                                                    TextureUsage.Storage | TextureUsage.Sampled));

        _outputTextureView = _device.Factory.CreateTextureView(_outputTexture);

        _resourceSet.UpdateSet(_outputTextureView, 1);
    }

    protected override void Destroy()
    {
        _outputTextureView?.Dispose();
        _outputTexture?.Dispose();

        _commandList.Dispose();
        _pipeline.Dispose();
        _shader.Dispose();
        _resourceSet.Dispose();
        _resourceLayout.Dispose();
        _buffer.Dispose();
    }
}
