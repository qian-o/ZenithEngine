using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using Tests.Core;
using MouseButtonEventArgs = Tests.Core.MouseButtonEventArgs;
using MouseMoveEventArgs = Tests.Core.MouseMoveEventArgs;

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
        public int Width;

        [FieldOffset(76)]
        public int Height;

        [FieldOffset(80)]
        public Vector3 Background;

        [FieldOffset(92)]
        public int AntiAliasing;

        [FieldOffset(96)]
        public int maxSteps;

        [FieldOffset(100)]
        public float Epsilon;
    }

    private readonly ViewController _viewController;

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

    private Camera _camera;

    private Vector2? lastMousePosition;

    public MainView(GraphicsDevice device, ImGuiController imGuiController) : base("Compute View")
    {
        _viewController = new ViewController(this)
        {
            UseDpiScale = false
        };
        _viewController.MouseDown += MouseDown;
        _viewController.MouseUp += MouseUp;
        _viewController.MouseMove += MouseMove;

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

        _camera = new()
        {
            Position = new Vector3(0.0f, 2.0f, 6.0f),
            Forward = new Vector3(0.0f, 0.0f, -1.0f),
            Right = new Vector3(1.0f, 0.0f, 0.0f),
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            NearPlane = 0.1f,
            FarPlane = 100.0f,
            Fov = MathF.PI / 4.0f,
            Width = 100,
            Height = 100,
            Background = new Vector3(0.7f, 0.7f, 0.9f),
            AntiAliasing = 4,
            maxSteps = 256,
            Epsilon = 0.0001f
        };
    }

    protected override void OnUpdate(UpdateEventArgs e)
    {
        _viewController.Update();

        if (ImGui.IsKeyDown(ImGuiKey.W))
        {
            _camera.Position += _camera.Forward * 0.1f;
        }

        if (ImGui.IsKeyDown(ImGuiKey.S))
        {
            _camera.Position -= _camera.Forward * 0.1f;
        }

        if (ImGui.IsKeyDown(ImGuiKey.A))
        {
            _camera.Position -= _camera.Right * 0.1f;
        }

        if (ImGui.IsKeyDown(ImGuiKey.D))
        {
            _camera.Position += _camera.Right * 0.1f;
        }

        if (ImGui.IsKeyDown(ImGuiKey.Q))
        {
            _camera.Position -= _camera.Up * 0.1f;
        }

        if (ImGui.IsKeyDown(ImGuiKey.E))
        {
            _camera.Position += _camera.Up * 0.1f;
        }

        if (ImGui.Begin("Properties"))
        {
            ImGui.DragFloat("Near Plane", ref _camera.NearPlane, 0.1f, 0.0f, 100.0f);
            ImGui.DragFloat("Far Plane", ref _camera.FarPlane, 0.1f, 0.0f, 100.0f);

            float fovDeg = _camera.Fov * 180.0f / MathF.PI;
            if (ImGui.DragFloat("Field of View", ref fovDeg, 0.1f, 0.0f, 180.0f))
            {
                _camera.Fov = fovDeg * MathF.PI / 180.0f;
            }

            ImGui.ColorEdit3("Background", ref _camera.Background);

            ImGui.DragInt("Anti Aliasing", ref _camera.AntiAliasing, 1, 1, 4);

            ImGui.DragInt("Max Steps", ref _camera.maxSteps, 1, 1, 1024);

            ImGui.End();
        }

        _device.UpdateBuffer(_buffer, 0, in _camera);
    }

    protected override void OnRender(RenderEventArgs e)
    {
        if (_outputTexture != null)
        {
            _commandList.Begin();

            _commandList.SetPipeline(_pipeline);
            _commandList.SetResourceSet(0, _resourceSet);

            _commandList.Dispatch((uint)Math.Ceiling(_outputTexture.Width / 32.0),
                                  (uint)Math.Ceiling(_outputTexture.Height / 32.0),
                                  1);

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

        _camera.Width = (int)e.Width;
        _camera.Height = (int)e.Height;
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

    private void MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button == ImGuiMouseButton.Right)
        {
            lastMousePosition = e.Position;
        }
    }

    private void MouseUp(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button == ImGuiMouseButton.Right)
        {
            lastMousePosition = null;
        }
    }

    private void MouseMove(object? sender, MouseMoveEventArgs e)
    {
        if (lastMousePosition.HasValue)
        {
            Vector2 delta = e.Position - lastMousePosition.Value;

            _camera.Forward = Vector3.TransformNormal(_camera.Forward, Matrix4x4.CreateFromAxisAngle(_camera.Up, -delta.X * 0.01f));
            _camera.Forward = Vector3.TransformNormal(_camera.Forward, Matrix4x4.CreateFromAxisAngle(_camera.Right, -delta.Y * 0.01f));

            _camera.Up = Vector3.Normalize(Vector3.Cross(_camera.Right, _camera.Forward));
            _camera.Right = Vector3.Normalize(Vector3.Cross(_camera.Forward, _camera.Up));

            _camera.Up.X = 0.0f;
            _camera.Right.Y = 0.0f;

            lastMousePosition = e.Position;
        }
    }
}
