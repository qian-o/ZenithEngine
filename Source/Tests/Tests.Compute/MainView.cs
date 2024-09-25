using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using Tests.Core;

namespace Tests.Compute;

internal sealed unsafe class MainView : View
{
    [StructLayout(LayoutKind.Explicit)]
    private struct Camera
    {
        [FieldOffset(0)]
        public Vector3 Position;

        [FieldOffset(16)]
        public Vector3 Forward;

        [FieldOffset(32)]
        public Vector3 Right;

        [FieldOffset(48)]
        public Vector3 Up;

        [FieldOffset(60)]
        public float NearPlane;

        [FieldOffset(64)]
        public float FarPlane;

        [FieldOffset(68)]
        public float Fov;

        [FieldOffset(72)]
        public uint Width;

        [FieldOffset(76)]
        public uint Height;

        [FieldOffset(80)]
        public Vector3 Background;

        [FieldOffset(92)]
        public uint AntiAliasing;

        [FieldOffset(96)]
        public uint maxSteps;

        [FieldOffset(100)]
        public float Epsilon;
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

        _buffer = device.Factory.CreateBuffer(new BufferDescription((uint)sizeof(Camera), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        _resourceLayout = device.Factory.CreateResourceLayout(new ResourceLayoutDescription(new ResourceLayoutElementDescription("camera", ResourceKind.UniformBuffer, ShaderStages.Compute),
                                                                                            new ResourceLayoutElementDescription("outputTexture", ResourceKind.StorageImage, ShaderStages.Compute)));

        _resourceSet = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout, _buffer));

        _shader = device.Factory.CompileHlslToSpirv(new ShaderDescription(ShaderStages.Compute, Encoding.UTF8.GetBytes(hlsl), "main")).First();

        _pipeline = device.Factory.CreateComputePipeline(new ComputePipelineDescription(_shader, [_resourceLayout]));

        _commandList = device.Factory.CreateComputeCommandList();
    }

    protected override void OnUpdate(UpdateEventArgs e)
    {
        if (_outputTexture != null)
        {
            Camera camera = new()
            {
                Position = new Vector3(0.0f, 2.0f, 6.0f),
                Forward = new Vector3(0.0f, 0.0f, -1.0f),
                Right = new Vector3(1.0f, 0.0f, 0.0f),
                Up = new Vector3(0.0f, 1.0f, 0.0f),
                NearPlane = 0.1f,
                FarPlane = 100.0f,
                Fov = MathF.PI / 4.0f,
                Width = _outputTexture.Width,
                Height = _outputTexture.Height,
                Background = new Vector3(0.7f, 0.7f, 0.9f),
                AntiAliasing = 2,
                maxSteps = 256,
                Epsilon = 0.0001f
            };

            _device.UpdateBuffer(_buffer, 0, in camera);
        }
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
