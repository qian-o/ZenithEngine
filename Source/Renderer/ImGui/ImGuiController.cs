using System.Numerics;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Renderer;

public unsafe class ImGuiController : DisposableObject
{
    private const string VertexShader = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 UV;
layout(location = 2) in vec4 Color;

layout(location = 0) out struct
{
    vec2 UV;
    vec4 Color;
}fsin;

layout(set = 0, std140, binding = 0) uniform UBO
{
    mat4 Projection;
}ubo;

void main()
{
    gl_Position = ubo.Projection * vec4(Position, 0, 1);
    fsin.UV = UV;
    fsin.Color = Color;
}";

    private const string FragmentShader = @"
#version 450

layout(location = 0) in struct
{
    vec2 UV;
    vec4 Color;
}fsin;

layout(location = 0) out vec4 fsout_Color;

layout(set = 0, binding = 1) uniform sampler FontSampler;
layout(set = 1, binding = 0) uniform texture2D FontTexture;

void main()
{
    fsout_Color = fsin.Color * texture(sampler2D(FontTexture, FontSampler), fsin.UV.st);
}";

    private readonly GraphicsDevice _graphicsDevice;
    private readonly ResourceFactory _factory;
    private readonly IView _view;
    private readonly IInputContext _input;
    private readonly ImGuiContextPtr _imGuiContext;

    private int _windowWidth;
    private int _windowHeight;
    private DeviceBuffer _vertexBuffer = null!;
    private DeviceBuffer _indexBuffer = null!;
    private DeviceBuffer _uboBuffer = null!;
    private ResourceLayout _layout0 = null!;
    private ResourceLayout _layout1 = null!;
    private ResourceSet _resourceSet = null!;
    private Pipeline _pipeline = null!;

    #region Constructors
    public ImGuiController(GraphicsDevice graphicsDevice,
                           IView view,
                           IInputContext input,
                           ImGuiFontConfig? imGuiFontConfig,
                           Action? onConfigureIO)
    {
        _graphicsDevice = graphicsDevice;
        _factory = _graphicsDevice.ResourceFactory;
        _view = view;
        _input = input;
        _imGuiContext = ImGui.CreateContext();
        _windowWidth = view.Size.X;
        _windowHeight = view.Size.Y;

        Initialize(imGuiFontConfig, onConfigureIO);
    }

    public ImGuiController(GraphicsDevice graphicsDevice,
                           IView view,
                           IInputContext input,
                           ImGuiFontConfig imGuiFontConfig) : this(graphicsDevice,
                                                                   view,
                                                                   input,
                                                                   imGuiFontConfig,
                                                                   null)
    {
    }

    public ImGuiController(GraphicsDevice graphicsDevice,
                           IView view,
                           IInputContext input,
                           Action onConfigureIO) : this(graphicsDevice,
                                                        view,
                                                        input,
                                                        null,
                                                        onConfigureIO)
    {
    }

    public ImGuiController(GraphicsDevice graphicsDevice,
                           IView view,
                           IInputContext input) : this(graphicsDevice,
                                                       view,
                                                       input,
                                                       null,
                                                       null)
    {
    }
    #endregion

    protected override void Destroy()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _uboBuffer.Dispose();
        _layout0.Dispose();
        _layout1.Dispose();
        _resourceSet.Dispose();
        _pipeline.Dispose();

        ImGui.DestroyContext(_imGuiContext);
    }

    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        io.DisplaySize = new Vector2(_windowWidth, _windowHeight);

        if (_windowWidth > 0 && _windowHeight > 0)
        {
            io.DisplayFramebufferScale = new Vector2(_view.FramebufferSize.X / _windowWidth, _view.FramebufferSize.Y / _windowHeight);
        }

        io.DeltaTime = deltaSeconds;
    }

    private void RecreateFontDeviceTexture()
    {
        ImGuiIOPtr io = ImGui.GetIO();

        byte* pixels;
        int width;
        int height;
        io.Fonts.GetTexDataAsRGBA32(&pixels, &width, &height);

        TextureDescription description = TextureDescription.Texture2D((uint)width,
                                                                      (uint)height,
                                                                      1,
                                                                      PixelFormat.R8G8B8A8UNorm,
                                                                      TextureUsage.Sampled);

        Texture fontTexture = _factory.CreateTexture(description);

        _graphicsDevice.UpdateTexture(fontTexture,
                                      pixels,
                                      (uint)(width * height * 4),
                                      0,
                                      0,
                                      0,
                                      (uint)width,
                                      (uint)height,
                                      1,
                                      0,
                                      0);
    }

    private void CreateDeviceResources()
    {
        _vertexBuffer = _factory.CreateBuffer(new BufferDescription(10000, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        _indexBuffer = _factory.CreateBuffer(new BufferDescription(2000, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
        _uboBuffer = _factory.CreateBuffer(new BufferDescription((uint)sizeof(Matrix4x4), BufferUsage.UniformBuffer));

        Shader[] shaders = _factory.CreateFromSpirv(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexShader), "main"),
                                                    new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentShader), "main"));

        VertexLayoutDescription vertexLayoutDescription = new(new VertexElementDescription("Position", VertexElementFormat.Float2),
                                                              new VertexElementDescription("UV", VertexElementFormat.Float2),
                                                              new VertexElementDescription("Color", VertexElementFormat.Byte4Norm));

        ResourceLayoutDescription set0 = new(new ResourceLayoutElementDescription("UBO", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                                             new ResourceLayoutElementDescription("FontSampler", ResourceKind.Sampler, ShaderStages.Fragment));

        ResourceLayoutDescription set1 = new(new ResourceLayoutElementDescription("FontTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment));

        _layout0 = _factory.CreateResourceLayout(set0);
        _layout1 = _factory.CreateResourceLayout(set1);
        _resourceSet = _factory.CreateResourceSet(new ResourceSetDescription(_layout0, _uboBuffer, _graphicsDevice.PointSampler));

        GraphicsPipelineDescription pipelineDescription = new()
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = DepthStencilStateDescription.Disabled,
            RasterizerState = RasterizerStateDescription.Default,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = [_layout0, _layout1],
            ShaderSet = new ShaderSetDescription([vertexLayoutDescription], shaders),
            Outputs = _graphicsDevice.Swapchain.OutputDescription
        };

        _pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);

        RecreateFontDeviceTexture();
    }

    private void Initialize(ImGuiFontConfig? imGuiFontConfig, Action? onConfigureIO)
    {
        ImGui.SetCurrentContext(_imGuiContext);
        ImGui.StyleColorsDark();

        ImGuizmo.SetImGuiContext(_imGuiContext);

        ImGuiIOPtr io = ImGui.GetIO();

        if (imGuiFontConfig.HasValue)
        {
            nint glyph_ranges = imGuiFontConfig.Value.GetGlyphRange?.Invoke(io) ?? 0;
            io.Fonts.AddFontFromFileTTF(imGuiFontConfig.Value.FontPath, imGuiFontConfig.Value.FontSize, null, (char*)glyph_ranges);
        }

        onConfigureIO?.Invoke();

        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        CreateDeviceResources();
        SetPerFrameImGuiData(1.0f / 60.0f);
    }
}
