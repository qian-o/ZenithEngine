using System.Numerics;
using System.Runtime.InteropServices;
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
    private readonly Dictionary<nint, ImGuiPlatform> _platformsByHandle;

    private readonly PlatformCreateWindow _createWindow;
    private readonly PlatformDestroyWindow _destroyWindow;
    private readonly PlatformShowWindow _showWindow;
    private readonly PlatformGetWindowPos _getWindowPos;
    private readonly PlatformSetWindowPos _setWindowPos;
    private readonly PlatformGetWindowSize _getWindowSize;
    private readonly PlatformSetWindowSize _setWindowSize;
    private readonly PlatformGetWindowFocus _getWindowFocus;
    private readonly PlatformSetWindowFocus _setWindowFocus;
    private readonly PlatformGetWindowMinimized _getWindowMinimized;
    private readonly PlatformSetWindowTitle _setWindowTitle;
    private readonly PlatformUpdateWindow _updateWindow;

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
        _platformsByHandle = [];

        _createWindow = CreateWindow;
        _destroyWindow = DestroyWindow;
        _showWindow = ShowWindow;
        _getWindowPos = GetWindowPos;
        _setWindowPos = SetWindowPos;
        _getWindowSize = GetWindowSize;
        _setWindowSize = SetWindowSize;
        _getWindowFocus = GetWindowFocus;
        _setWindowFocus = SetWindowFocus;
        _getWindowMinimized = GetWindowMinimized;
        _setWindowTitle = SetWindowTitle;
        _updateWindow = UpdateWindow;

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
        ImGui.EndFrame();

        _imGuiRenderer.RenderImDrawData(commandList, ImGui.GetDrawData());

        ImGui.UpdatePlatformWindows();
        foreach (ImGuiPlatform platform in _platforms)
        {
            ImGuiViewport* vp = ImGui.FindViewportByPlatformHandle((void*)platform.Handle);

            commandList.SetFramebuffer(platform.Swapchain!.Framebuffer);

            _imGuiRenderer.RenderImDrawData(commandList, vp->DrawData);
        }
    }

    public void PlatformSwapBuffers()
    {
        foreach (ImGuiPlatform platform in _platforms)
        {
            platform.SwapBuffers();
        }
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
        ImGuizmo.SetImGuiContext(_imGuiContext);

        ImGui.StyleColorsDark();

        ImGuiIOPtr io = ImGui.GetIO();

        if (imGuiFontConfig.HasValue)
        {
            nint glyph_ranges = imGuiFontConfig.Value.GetGlyphRange?.Invoke(io) ?? 0;
            io.Fonts.AddFontFromFileTTF(imGuiFontConfig.Value.FontPath, imGuiFontConfig.Value.FontSize, null, (char*)glyph_ranges);
        }

        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

        onConfigureIO?.Invoke();

        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
        io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.BackendFlags |= ImGuiBackendFlags.PlatformHasViewports;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasViewports;

        InitializePlatform();
    }

    private void InitializePlatform()
    {
        ImGuiViewport* mainViewport = ImGui.GetMainViewport();
        ImGuiPlatform mainPlatform = new(mainViewport, _graphicsWindow, _graphicsDevice);

        _platformsByHandle.Add((nint)mainViewport->PlatformHandle, mainPlatform);

        InitializePlatformCallbacks();
    }

    private void InitializePlatformCallbacks()
    {
        ImGuiPlatformIOPtr platformIO = ImGui.GetPlatformIO();

        platformIO.PlatformCreateWindow = (void*)Marshal.GetFunctionPointerForDelegate(_createWindow);
        platformIO.PlatformDestroyWindow = (void*)Marshal.GetFunctionPointerForDelegate(_destroyWindow);
        platformIO.PlatformShowWindow = (void*)Marshal.GetFunctionPointerForDelegate(_showWindow);
        platformIO.PlatformGetWindowPos = (void*)Marshal.GetFunctionPointerForDelegate(_getWindowPos);
        platformIO.PlatformSetWindowPos = (void*)Marshal.GetFunctionPointerForDelegate(_setWindowPos);
        platformIO.PlatformGetWindowSize = (void*)Marshal.GetFunctionPointerForDelegate(_getWindowSize);
        platformIO.PlatformSetWindowSize = (void*)Marshal.GetFunctionPointerForDelegate(_setWindowSize);
        platformIO.PlatformGetWindowFocus = (void*)Marshal.GetFunctionPointerForDelegate(_getWindowFocus);
        platformIO.PlatformSetWindowFocus = (void*)Marshal.GetFunctionPointerForDelegate(_setWindowFocus);
        platformIO.PlatformGetWindowMinimized = (void*)Marshal.GetFunctionPointerForDelegate(_getWindowMinimized);
        platformIO.PlatformSetWindowTitle = (void*)Marshal.GetFunctionPointerForDelegate(_setWindowTitle);
        platformIO.PlatformUpdateWindow = (void*)Marshal.GetFunctionPointerForDelegate(_updateWindow);
    }

    private void CreateWindow(ImGuiViewport* vp)
    {
        ImGuiPlatform platform = new(vp, _graphicsDevice);

        _platforms.Add(platform);
        _platformsByHandle.Add((nint)vp->PlatformHandle, platform);
    }

    private void DestroyWindow(ImGuiViewport* vp)
    {
        if (vp->PlatformHandle == null)
        {
            return;
        }

        ImGuiPlatform platform = _platformsByHandle[(nint)vp->PlatformHandle];

        _platforms.Remove(platform);
        _platformsByHandle.Remove((nint)vp->PlatformHandle);

        platform.Dispose();
    }

    private void ShowWindow(ImGuiViewport* vp)
    {
        ImGuiPlatform platform = _platformsByHandle[(nint)vp->PlatformHandle];

        platform.Show();
    }

    private Vector2* GetWindowPos(Vector2* pos, ImGuiViewport* viewport)
    {
        ImGuiPlatform platform = _platformsByHandle[(nint)viewport->PlatformHandle];

        *pos = platform.Position;

        return pos;
    }

    private void SetWindowPos(ImGuiViewport* vp, Vector2 pos)
    {
        ImGuiPlatform platform = _platformsByHandle[(nint)vp->PlatformHandle];

        platform.Position = pos;
    }

    private Vector2* GetWindowSize(Vector2* size, ImGuiViewport* viewport)
    {
        ImGuiPlatform platform = _platformsByHandle[(nint)viewport->PlatformHandle];

        *size = platform.Size;

        return size;
    }

    private void SetWindowSize(ImGuiViewport* vp, Vector2 size)
    {
        ImGuiPlatform platform = _platformsByHandle[(nint)vp->PlatformHandle];

        platform.Size = size;
    }

    private byte GetWindowFocus(ImGuiViewport* vp)
    {
        ImGuiPlatform platform = _platformsByHandle[(nint)vp->PlatformHandle];

        return platform.IsFocused;
    }

    private void SetWindowFocus(ImGuiViewport* vp)
    {
        ImGuiPlatform platform = _platformsByHandle[(nint)vp->PlatformHandle];

        platform.Focus();
    }

    private byte GetWindowMinimized(ImGuiViewport* vp)
    {
        ImGuiPlatform platform = _platformsByHandle[(nint)vp->PlatformHandle];

        return platform.IsMinimized;
    }

    private void SetWindowTitle(ImGuiViewport* vp, byte* str)
    {
        ImGuiPlatform platform = _platformsByHandle[(nint)vp->PlatformHandle];

        platform.Title = Marshal.PtrToStringAnsi((nint)str)!;
    }

    private void UpdateWindow(ImGuiViewport* vp)
    {
        ImGuiPlatform platform = _platformsByHandle[(nint)vp->PlatformHandle];

        platform.Update();
    }
}
