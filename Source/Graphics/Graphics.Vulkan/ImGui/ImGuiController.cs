using Graphics.Core;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;

namespace Graphics.Vulkan;

public unsafe class ImGuiController : DisposableObject
{
    private readonly GraphicsWindow _graphicsWindow;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ImGuiContextPtr _imGuiContext;
    private readonly ImGuiRenderer _imGuiRenderer;
    private readonly List<ImGuiPlatform> _platforms;

    #region Constructors
    public ImGuiController(GraphicsWindow graphicsWindow,
                           GraphicsDevice graphicsDevice,
                           ColorSpaceHandling colorSpaceHandling,
                           ImGuiFontConfig? imGuiFontConfig,
                           Action? onConfigureIO)
    {
        _graphicsWindow = graphicsWindow;
        _graphicsDevice = graphicsDevice;
        _imGuiContext = ImGui.CreateContext();
        _imGuiRenderer = new ImGuiRenderer(graphicsDevice, colorSpaceHandling);
        _platforms = [];

        Initialize(imGuiFontConfig, onConfigureIO);
    }

    public ImGuiController(GraphicsWindow graphicsWindow,
                           GraphicsDevice graphicsDevice,
                           ImGuiFontConfig imGuiFontConfig) : this(graphicsWindow,
                                                                   graphicsDevice,
                                                                   ColorSpaceHandling.Legacy,
                                                                   imGuiFontConfig,
                                                                   null)
    {
    }

    public ImGuiController(GraphicsWindow graphicsWindow,
                           GraphicsDevice graphicsDevice) : this(graphicsWindow,
                                                                 graphicsDevice,
                                                                 ColorSpaceHandling.Legacy,
                                                                 null,
                                                                 null)
    {
    }
    #endregion

    public void Update(float deltaSeconds)
    {
        SetPerFrameImGuiData(deltaSeconds);

        ImGui.NewFrame();
        ImGuizmo.BeginFrame();

        ImGui.DockSpaceOverViewport();

        ImGui.ShowDemoWindow();
    }

    public void Render(CommandList commandList)
    {
        ImGui.Render();
        _imGuiRenderer.RenderImDrawData(commandList, ImGui.GetDrawData());

        ImGui.UpdatePlatformWindows();
        ImGui.RenderPlatformWindowsDefault();
    }

    public nint GetOrCreateImGuiBinding(ResourceFactory factory, TextureView textureView)
    {
        return _imGuiRenderer.GetOrCreateImGuiBinding(factory, textureView);
    }

    public nint GetOrCreateImGuiBinding(ResourceFactory factory, Texture texture)
    {
        return _imGuiRenderer.GetOrCreateImGuiBinding(factory, texture);
    }

    public void RemoveImGuiBinding(nint binding)
    {
        _imGuiRenderer.RemoveImGuiBinding(binding);
    }

    protected override void Destroy()
    {
        foreach (ImGuiPlatform platform in _platforms)
        {
            platform.Dispose();
        }

        ImGui.DestroyPlatformWindows();

        _imGuiRenderer.Dispose();

        ImGui.DestroyContext(_imGuiContext);
    }

    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        io.DisplaySize = _graphicsWindow.Size;

        if (_graphicsWindow.Size.X > 0 && _graphicsWindow.Size.Y > 0)
        {
            io.DisplayFramebufferScale = _graphicsWindow.FramebufferSize / _graphicsWindow.Size;
        }

        io.DeltaTime = deltaSeconds;
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

        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        // io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

        onConfigureIO?.Invoke();

        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
        io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.BackendFlags |= ImGuiBackendFlags.PlatformHasViewports;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasViewports;

        _platforms.Add(new ImGuiPlatform(_graphicsWindow, _graphicsDevice));
    }
}
