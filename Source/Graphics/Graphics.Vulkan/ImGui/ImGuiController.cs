using System.Numerics;
using System.Runtime.InteropServices;
using Graphics.Core;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Silk.NET.Input;
using Silk.NET.SDL;
using Window = Graphics.Core.Window;

namespace Graphics.Vulkan;

public unsafe class ImGuiController : DisposableObject
{
    private readonly Window _window;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ImGuiContextPtr _imGuiContext;
    private readonly ImGuiRenderer _imGuiRenderer;
    private readonly List<ImGuiPlatform> _platforms;
    private readonly Dictionary<nint, ImGuiPlatform> _platformsByHandle;
    private readonly Dictionary<ImGuiMouseCursor, nint> _mouseCursors;

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
    private readonly PlatformSetWindowAlpha _setWindowAlpha;
    private readonly PlatformUpdateWindow _updateWindow;

    #region Constructors
    public ImGuiController(Window window,
                           GraphicsDevice graphicsDevice,
                           ColorSpaceHandling colorSpaceHandling,
                           ImGuiFontConfig? imGuiFontConfig,
                           Action? onConfigureIO)
    {
        _window = window;
        _graphicsDevice = graphicsDevice;
        _imGuiContext = ImGui.CreateContext();
        _imGuiRenderer = new ImGuiRenderer(graphicsDevice, colorSpaceHandling);
        _platforms = [];
        _platformsByHandle = [];
        _mouseCursors = [];

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
        _setWindowAlpha = SetWindowAlpha;
        _updateWindow = UpdateWindow;

        Initialize(imGuiFontConfig, onConfigureIO);
    }

    public ImGuiController(Window window,
                           GraphicsDevice graphicsDevice,
                           ImGuiFontConfig imGuiFontConfig) : this(window,
                                                                   graphicsDevice,
                                                                   ColorSpaceHandling.Legacy,
                                                                   imGuiFontConfig,
                                                                   null)
    {
    }

    public ImGuiController(Window window,
                           GraphicsDevice graphicsDevice) : this(window,
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

        UpdateMouseState();
        UpdateMouseCursor();

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
            commandList.SetFramebuffer(platform.Swapchain!.Framebuffer);

            _imGuiRenderer.RenderImDrawData(commandList, platform.Viewport->DrawData);
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
        foreach (nint cursor in _mouseCursors.Values)
        {
            Window.FreeCursor((Cursor*)cursor);
        }

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

        io.DisplaySize = _window.Size;

        if (_window.Size.X > 0 && _window.Size.Y > 0)
        {
            io.DisplayFramebufferScale = _window.FramebufferSize / _window.Size;
        }

        io.DeltaTime = deltaSeconds;
    }

    private static void UpdateMouseState()
    {
        ImGuiIOPtr io = ImGui.GetIO();

        MouseButton[] mouseButtons = Window.GetGlobalMouseState(out Vector2 position);

        io.AddMouseButtonEvent(0, mouseButtons.Contains(MouseButton.Left));
        io.AddMouseButtonEvent(1, mouseButtons.Contains(MouseButton.Right));
        io.AddMouseButtonEvent(2, mouseButtons.Contains(MouseButton.Middle));
        io.AddMouseButtonEvent(3, mouseButtons.Contains(MouseButton.Button4));
        io.AddMouseButtonEvent(4, mouseButtons.Contains(MouseButton.Button5));

        io.AddMousePosEvent(position.X, position.Y);
    }

    private void UpdateMouseCursor()
    {
        ImGuiIOPtr io = ImGui.GetIO();

        if (io.ConfigFlags.HasFlag(ImGuiConfigFlags.NoMouseCursorChange))
        {
            return;
        }

        ImGuiMouseCursor imguiCursor = ImGui.GetMouseCursor();

        Window.SetCursor((Cursor*)_mouseCursors[imguiCursor]);
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

        foreach (ImGuiMouseCursor imGuiMouseCursor in Enum.GetValues<ImGuiMouseCursor>())
        {
            _mouseCursors.Add(imGuiMouseCursor, (nint)Window.CreateCursor(MapMouseCursor(imGuiMouseCursor)));
        }

        InitializePlatform();
    }

    private void InitializePlatform()
    {
        ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();
        ImGuiPlatform mainPlatform = new(mainViewport, _window, _graphicsDevice);

        _platformsByHandle.Add((nint)mainViewport.PlatformHandle, mainPlatform);

        InitializePlatformMonitors();
        InitializePlatformCallbacks();
    }

    private void InitializePlatformMonitors()
    {
        ImGuiPlatformIOPtr platformIO = ImGui.GetPlatformIO();

        int displayCount = Window.GetDisplayCount();

        platformIO.Monitors.Size = displayCount;
        platformIO.Monitors.Capacity = displayCount;
        platformIO.Monitors.Data = (ImGuiPlatformMonitor*)Marshal.AllocHGlobal(Marshal.SizeOf<ImGuiPlatformMonitor>() * displayCount);

        for (int i = 0; i < displayCount; i++)
        {
            Display display = Window.GetDisplay(i);

            ImGuiPlatformMonitor monitor = new()
            {
                MainPos = display.MainPosition,
                MainSize = display.MainSize,
                WorkPos = display.WorkPosition,
                WorkSize = display.WorkSize,
                DpiScale = display.DpiScale
            };

            platformIO.Monitors.Data[i] = monitor;
        }
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
        platformIO.PlatformSetWindowAlpha = (void*)Marshal.GetFunctionPointerForDelegate(_setWindowAlpha);
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

    private void SetWindowAlpha(ImGuiViewport* vp, float alpha)
    {
        ImGuiPlatform platform = _platformsByHandle[(nint)vp->PlatformHandle];

        platform.Alpha = alpha;
    }

    private void UpdateWindow(ImGuiViewport* vp)
    {
        ImGuiPlatform platform = _platformsByHandle[(nint)vp->PlatformHandle];

        platform.Update();
    }

    private static SystemCursor MapMouseCursor(ImGuiMouseCursor imguiCursor)
    {
        return imguiCursor switch
        {
            ImGuiMouseCursor.TextInput => SystemCursor.SystemCursorIbeam,
            ImGuiMouseCursor.ResizeAll => SystemCursor.SystemCursorSizeall,
            ImGuiMouseCursor.ResizeNs => SystemCursor.SystemCursorSizens,
            ImGuiMouseCursor.ResizeEw => SystemCursor.SystemCursorSizewe,
            ImGuiMouseCursor.ResizeNesw => SystemCursor.SystemCursorSizenesw,
            ImGuiMouseCursor.ResizeNwse => SystemCursor.SystemCursorSizenwse,
            ImGuiMouseCursor.Hand => SystemCursor.SystemCursorHand,
            ImGuiMouseCursor.NotAllowed => SystemCursor.SystemCursorNo,
            _ => SystemCursor.SystemCursorArrow
        };
    }
}
