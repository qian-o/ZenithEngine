﻿using System.Numerics;
using System.Runtime.InteropServices;
using Graphics.Core;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Hexa.NET.ImNodes;
using Hexa.NET.ImPlot;
using Silk.NET.Input;
using Silk.NET.SDL;
using DearImGui = Hexa.NET.ImGui.ImGui;

namespace Graphics.Vulkan.ImGui;

public unsafe class ImGuiController : DisposableObject
{
    private readonly SdlWindow _window;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ImGuiContextPtr _imGuiContext;
    private readonly ImPlotContextPtr _imPlotContext;
    private readonly ImNodesContextPtr _imNodesContext;
    private readonly ImGuiFontConfig _imGuiFontConfig;
    private readonly ImGuiSizeConfig _imGuiSizeConfig;
    private readonly Dictionary<float, ImFontPtr> _dpiScaleFonts;
    private readonly Dictionary<float, ImGuiSizeConfig> _dpiScaleSizes;
    private readonly List<ImGuiPlatform> _platforms;
    private readonly Dictionary<nint, ImGuiPlatform> _platformsByHandle;
    private readonly Dictionary<ImGuiMouseCursor, nint> _mouseCursors;
    private readonly ImGuiRenderer _imGuiRenderer;

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
    private readonly PlatformGetWindowDpiScale _getWindowDpiScale;
    private readonly PlatformOnChangedViewport _onChangedViewport;
    private readonly PlatformSetImeDataFn _setImeData;

    private bool _frameBegun;
    private float _currentDpiScale;

    #region Constructors
    public ImGuiController(SdlWindow window,
                           GraphicsDevice graphicsDevice,
                           ColorSpaceHandling colorSpaceHandling,
                           ImGuiFontConfig imGuiFontConfig,
                           ImGuiSizeConfig imGuiSizeConfig,
                           Action? onConfigureIO)
    {
        _window = window;
        _graphicsDevice = graphicsDevice;
        _imGuiContext = DearImGui.CreateContext();
        _imPlotContext = ImPlot.CreateContext();
        _imNodesContext = ImNodes.CreateContext();
        _imGuiFontConfig = imGuiFontConfig;
        _imGuiSizeConfig = imGuiSizeConfig;
        _dpiScaleFonts = [];
        _dpiScaleSizes = [];
        _platforms = [];
        _platformsByHandle = [];
        _mouseCursors = [];
        _imGuiRenderer = new ImGuiRenderer(graphicsDevice, colorSpaceHandling);

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
        _getWindowDpiScale = GetWindowDpiScale;
        _onChangedViewport = OnChangedViewport;
        _setImeData = SetImeData;

        Initialize(onConfigureIO);
    }

    public ImGuiController(SdlWindow window,
                           GraphicsDevice graphicsDevice,
                           ImGuiFontConfig imGuiFontConfig,
                           ImGuiSizeConfig imGuiSizeConfig) : this(window,
                                                                   graphicsDevice,
                                                                   ColorSpaceHandling.Legacy,
                                                                   imGuiFontConfig,
                                                                   imGuiSizeConfig,
                                                                   null)
    {
    }

    public ImGuiController(SdlWindow window,
                           GraphicsDevice graphicsDevice) : this(window,
                                                                 graphicsDevice,
                                                                 ColorSpaceHandling.Legacy,
                                                                 ImGuiFontConfig.Default,
                                                                 ImGuiSizeConfig.Default,
                                                                 null)
    {
    }
    #endregion

    public void Update(float deltaSeconds)
    {
        if (_frameBegun)
        {
            DearImGui.Render();
        }

        DearImGui.SetCurrentContext(_imGuiContext);
        ImPlot.SetCurrentContext(_imPlotContext);
        ImNodes.SetCurrentContext(_imNodesContext);

        ImPlot.SetImGuiContext(_imGuiContext);
        ImNodes.SetImGuiContext(_imGuiContext);
        ImGuizmo.SetImGuiContext(_imGuiContext);

        SetPerFrameImGuiData(deltaSeconds);

        UpdateMouseState();
        UpdateMouseCursor();

        DearImGui.NewFrame();
        ImGuizmo.BeginFrame();

        DearImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode, null);

        _frameBegun = true;
    }

    public void Render(CommandList commandList)
    {
        if (_frameBegun)
        {
            DearImGui.Render();

            _imGuiRenderer.RenderImDrawData(commandList, DearImGui.GetDrawData());

            DearImGui.UpdatePlatformWindows();

            foreach (ImGuiPlatform platform in _platforms)
            {
                commandList.SetFramebuffer(platform.Swapchain!.Framebuffer);

                _imGuiRenderer.RenderImDrawData(commandList, platform.Viewport->DrawData);
            }

            _frameBegun = false;
        }
    }

    public void PlatformSwapBuffers()
    {
        foreach (ImGuiPlatform platform in _platforms)
        {
            platform.SwapBuffers();
        }
    }

    public nint GetBinding(ResourceFactory factory, TextureView textureView)
    {
        return _imGuiRenderer.GetBinding(factory, textureView);
    }

    public nint GetBinding(ResourceFactory factory, Texture texture)
    {
        return _imGuiRenderer.GetBinding(factory, texture);
    }

    public void RemoveBinding(nint binding)
    {
        _imGuiRenderer.RemoveBinding(binding);
    }

    protected override void Destroy()
    {
        _imGuiRenderer.Dispose();

        foreach (nint cursor in _mouseCursors.Values)
        {
            SdlWindow.FreeCursor((Cursor*)cursor);
        }

        foreach (ImGuiPlatform platform in _platforms)
        {
            platform.Dispose();
        }

        _mouseCursors.Clear();
        _platforms.Clear();

        ImNodes.DestroyContext(_imNodesContext);
        ImPlot.DestroyContext(_imPlotContext);
        DearImGui.DestroyContext(_imGuiContext);
    }

    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        ImGuiIOPtr io = DearImGui.GetIO();

        io.DisplaySize = _window.FramebufferSize;
        io.DisplayFramebufferScale = _window.FramebufferSize / _window.Size;

        io.DeltaTime = deltaSeconds;
    }

    private static void UpdateMouseState()
    {
        if (SdlWindow.IsMouseFocusOnWindow())
        {
            ImGuiIOPtr io = DearImGui.GetIO();

            MouseButton[] mouseButtons = SdlWindow.GetGlobalMouseState(out Vector2 position);

            io.AddMouseButtonEvent((int)ImGuiMouseButton.Left, mouseButtons.Contains(MouseButton.Left));
            io.AddMouseButtonEvent((int)ImGuiMouseButton.Right, mouseButtons.Contains(MouseButton.Right));
            io.AddMouseButtonEvent((int)ImGuiMouseButton.Middle, mouseButtons.Contains(MouseButton.Middle));
            io.AddMouseButtonEvent((int)ImGuiMouseButton.Count, mouseButtons.Contains(MouseButton.Button4));

            io.AddMousePosEvent(position.X, position.Y);
        }
    }

    private void UpdateMouseCursor()
    {
        ImGuiIOPtr io = DearImGui.GetIO();

        if (io.ConfigFlags.HasFlag(ImGuiConfigFlags.NoMouseCursorChange))
        {
            return;
        }

        ImGuiMouseCursor imguiCursor = DearImGui.GetMouseCursor();

        SdlWindow.SetCursor((Cursor*)_mouseCursors[imguiCursor]);
    }

    private void Initialize(Action? onConfigureIO)
    {
        DearImGui.SetCurrentContext(_imGuiContext);
        ImPlot.SetCurrentContext(_imPlotContext);
        ImNodes.SetCurrentContext(_imNodesContext);

        ImPlot.SetImGuiContext(_imGuiContext);
        ImNodes.SetImGuiContext(_imGuiContext);
        ImGuizmo.SetImGuiContext(_imGuiContext);

        DearImGui.StyleColorsDark();

        ImGuiIOPtr io = DearImGui.GetIO();

        io.Fonts.Clear();

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
            _mouseCursors.Add(imGuiMouseCursor, (nint)SdlWindow.CreateCursor(MapMouseCursor(imGuiMouseCursor)));
        }

        InitializePlatform();
    }

    private void InitializePlatform()
    {
        ImGuiViewportPtr mainViewport = DearImGui.GetMainViewport();
        ImGuiPlatform mainPlatform = new(mainViewport, _window, _graphicsDevice);

        _platformsByHandle.Add((nint)mainViewport.PlatformHandle, mainPlatform);

        InitializePlatformDpiScale();
        InitializePlatformMonitors();
        InitializePlatformCallbacks();
    }

    private void InitializePlatformDpiScale()
    {
        ImGuiIOPtr io = DearImGui.GetIO();

        int displayCount = SdlWindow.GetDisplayCount();

        for (int i = 0; i < displayCount; i++)
        {
            Display display = SdlWindow.GetDisplay(i);

            ImFontPtr fontPtr;
            if (_imGuiFontConfig.IsDefault)
            {
                fontPtr = io.Fonts.AddFontDefault();
            }
            else
            {
                char* glyph_ranges = _imGuiFontConfig.GetGlyphRange != null ? (char*)_imGuiFontConfig.GetGlyphRange(io) : io.Fonts.GetGlyphRangesDefault();

                fontPtr = io.Fonts.AddFontFromFileTTF(_imGuiFontConfig.FontPath, Convert.ToInt32(_imGuiFontConfig.FontSize * display.DpiScale), null, glyph_ranges);
            }

            _dpiScaleFonts.Add(display.DpiScale, fontPtr);

            ImGuiSizeConfig sizeConfig = _imGuiSizeConfig.Scale(display.DpiScale);

            _dpiScaleSizes.Add(display.DpiScale, sizeConfig);
        }

        _imGuiRenderer.RecreateFontDeviceTexture();
    }

    private void InitializePlatformMonitors()
    {
        ImGuiPlatformIOPtr platformIO = DearImGui.GetPlatformIO();

        platformIO.Monitors.Resize(SdlWindow.GetDisplayCount());

        for (int i = 0; i < platformIO.Monitors.Size; i++)
        {
            Display display = SdlWindow.GetDisplay(i);

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
        ImGuiPlatformIOPtr platformIO = DearImGui.GetPlatformIO();

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
        platformIO.PlatformGetWindowDpiScale = (void*)Marshal.GetFunctionPointerForDelegate(_getWindowDpiScale);
        platformIO.PlatformOnChangedViewport = (void*)Marshal.GetFunctionPointerForDelegate(_onChangedViewport);
        platformIO.PlatformSetImeDataFn = (void*)Marshal.GetFunctionPointerForDelegate(_setImeData);
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

    private float GetWindowDpiScale(ImGuiViewport* vp)
    {
        ImGuiPlatform platform = _platformsByHandle[(nint)vp->PlatformHandle];

        return platform.DpiScale;
    }

    private void OnChangedViewport(ImGuiViewport* vp)
    {
        // If the platform window has not been created yet, do nothing.
        if (vp->PlatformWindowCreated == 0)
        {
            return;
        }

        if (vp->DpiScale != _currentDpiScale)
        {
            _dpiScaleSizes[vp->DpiScale].Apply(DearImGui.GetStyle());

            _currentDpiScale = vp->DpiScale;
        }

        DearImGui.SetCurrentFont(_dpiScaleFonts[vp->DpiScale]);
    }

    private void SetImeData(ImGuiContext* ctx, ImGuiViewport* viewport, ImGuiPlatformImeData* data)
    {
        if (data->WantVisible == 1)
        {
            int x = Convert.ToInt32(data->InputPos.X - viewport->Pos.X);
            int y = Convert.ToInt32(data->InputPos.Y - viewport->Pos.Y);
            int w = 1;
            int h = Convert.ToInt32(data->InputLineHeight);

            SdlWindow.SetTextIputRect(x, y, w, h);
        }
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
