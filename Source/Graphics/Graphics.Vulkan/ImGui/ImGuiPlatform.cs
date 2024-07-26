using System.Numerics;
using Graphics.Core;
using Hexa.NET.ImGui;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Graphics.Vulkan;

internal sealed unsafe class ImGuiPlatform : DisposableObject
{
    private readonly ImGuiViewport* _viewport;
    private readonly GWindow _gWindow;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly bool _isExternalPlatform;

    private Swapchain? _swapchain;

    public ImGuiPlatform(ImGuiViewport* viewport, GWindow gWindow, GraphicsDevice graphicsDevice)
    {
        _viewport = viewport;
        _gWindow = gWindow;
        _graphicsDevice = graphicsDevice;
        _isExternalPlatform = true;

        Initialize();
    }

    public ImGuiPlatform(ImGuiViewport* viewport, GraphicsDevice graphicsDevice)
    {
        _viewport = viewport;
        _gWindow = GWindow.CreateWindowByVulkan();
        _graphicsDevice = graphicsDevice;
        _isExternalPlatform = false;

        Initialize();
    }

    public ImGuiViewport* Viewport => _viewport;

    public Swapchain? Swapchain => _swapchain;

    public string Title { get => _gWindow.Title; set => _gWindow.Title = value; }

    public Vector2 Position { get => _gWindow.Position; set => _gWindow.Position = value; }

    public Vector2 Size { get => _gWindow.Size; set => _gWindow.Size = value; }

    public byte IsFocused => _gWindow.IsFocused ? (byte)1 : (byte)0;

    public byte IsMinimized => _gWindow.WindowState == WindowState.Minimized ? (byte)1 : (byte)0;

    public void Show()
    {
        if (_isExternalPlatform)
        {
            return;
        }

        _gWindow.Show();
    }

    public void Focus()
    {
        _gWindow.Focus();
    }

    public void Update()
    {
        if (_isExternalPlatform)
        {
            return;
        }

        _gWindow.PollEvents();

        // sdl not trigger Resize event when size is changed actively.
        if (_swapchain!.Width != _gWindow.FramebufferSize.X || _swapchain!.Height != _gWindow.FramebufferSize.Y)
        {
            _swapchain.Resize((uint)_gWindow.FramebufferSize.X, (uint)_gWindow.FramebufferSize.Y);
        }
    }

    public void SwapBuffers()
    {
        if (_isExternalPlatform)
        {
            return;
        }

        _graphicsDevice.SwapBuffers(_swapchain!);
    }

    protected override void Destroy()
    {
        _swapchain?.Dispose();

        Unregister();

        if (!_isExternalPlatform)
        {
            _gWindow.Dispose();
        }
    }

    private void Initialize()
    {
        _viewport->PlatformHandle = (void*)_gWindow.Handle;

        if (!_isExternalPlatform)
        {
            if (_viewport->Flags.HasFlag(ImGuiViewportFlags.NoTaskBarIcon))
            {
                _gWindow.ShowInTaskbar = false;
            }

            if (_viewport->Flags.HasFlag(ImGuiViewportFlags.NoDecoration))
            {
                _gWindow.WindowBorder = WindowBorder.Hidden;
            }

            if (_viewport->Flags.HasFlag(ImGuiViewportFlags.TopMost))
            {
                _gWindow.TopMost = true;
            }

            SwapchainDescription swapchainDescription = new()
            {
                Target = _gWindow.VkSurface!,
                Width = (uint)_gWindow.FramebufferSize.X,
                Height = (uint)_gWindow.FramebufferSize.Y,
                DepthFormat = _graphicsDevice.GetBestDepthFormat()
            };

            _swapchain = _graphicsDevice.ResourceFactory.CreateSwapchain(in swapchainDescription);
        }

        Register();
    }

    private void Register()
    {
        _gWindow.MouseDown += MouseDown;
        _gWindow.MouseUp += MouseUp;
        _gWindow.MouseMove += MouseMove;
        _gWindow.MouseWheel += MouseWheel;
        _gWindow.KeyDown += KeyDown;
        _gWindow.KeyUp += KeyUp;
        _gWindow.KeyChar += KeyChar;
        _gWindow.Move += Move;
        _gWindow.Resize += Resize;
        _gWindow.Closing += Closing;
    }

    private void Unregister()
    {
        _gWindow.MouseDown -= MouseDown;
        _gWindow.MouseUp -= MouseUp;
        _gWindow.MouseMove -= MouseMove;
        _gWindow.MouseWheel -= MouseWheel;
        _gWindow.KeyDown -= KeyDown;
        _gWindow.KeyUp -= KeyUp;
        _gWindow.KeyChar -= KeyChar;
        _gWindow.Move -= Move;
        _gWindow.Resize -= Resize;
        _gWindow.Closing -= Closing;
    }

    private static bool TryMapMouseButton(MouseButton button, out int result)
    {
        result = button switch
        {
            MouseButton.Left => 0,
            MouseButton.Right => 1,
            MouseButton.Middle => 2,
            MouseButton.Button4 => 3,
            MouseButton.Button5 => 4,
            MouseButton.Button6 => 5,
            MouseButton.Button7 => 6,
            MouseButton.Button8 => 7,
            _ => -1
        };

        return result != -1;
    }

    private static bool TryMapKey(Key key, out ImGuiKey result)
    {
        static ImGuiKey KeyToImGuiKeyShortcut(Key keyToConvert, Key startKey1, ImGuiKey startKey2)
        {
            int changeFromStart1 = (int)keyToConvert - (int)startKey1;
            return startKey2 + changeFromStart1;
        }

        result = key switch
        {
            >= Key.F1 and <= Key.F24 => KeyToImGuiKeyShortcut(key, Key.F1, ImGuiKey.F1),
            >= Key.Keypad0 and <= Key.Keypad9 => KeyToImGuiKeyShortcut(key, Key.Keypad0, ImGuiKey.Keypad0),
            >= Key.A and <= Key.Z => KeyToImGuiKeyShortcut(key, Key.A, ImGuiKey.A),
            >= Key.Number0 and <= Key.Number9 => KeyToImGuiKeyShortcut(key, Key.Number0, ImGuiKey.Key0),
            Key.ShiftLeft or Key.ShiftRight => ImGuiKey.ModShift,
            Key.ControlLeft or Key.ControlRight => ImGuiKey.ModCtrl,
            Key.AltLeft or Key.AltRight => ImGuiKey.ModAlt,
            Key.SuperLeft or Key.SuperRight => ImGuiKey.ModSuper,
            Key.Menu => ImGuiKey.Menu,
            Key.Up => ImGuiKey.UpArrow,
            Key.Down => ImGuiKey.DownArrow,
            Key.Left => ImGuiKey.LeftArrow,
            Key.Right => ImGuiKey.RightArrow,
            Key.Enter => ImGuiKey.Enter,
            Key.Escape => ImGuiKey.Escape,
            Key.Space => ImGuiKey.Space,
            Key.Tab => ImGuiKey.Tab,
            Key.Backspace => ImGuiKey.Backspace,
            Key.Insert => ImGuiKey.Insert,
            Key.Delete => ImGuiKey.Delete,
            Key.PageUp => ImGuiKey.PageUp,
            Key.PageDown => ImGuiKey.PageDown,
            Key.Home => ImGuiKey.Home,
            Key.End => ImGuiKey.End,
            Key.CapsLock => ImGuiKey.CapsLock,
            Key.ScrollLock => ImGuiKey.ScrollLock,
            Key.PrintScreen => ImGuiKey.PrintScreen,
            Key.Pause => ImGuiKey.Pause,
            Key.NumLock => ImGuiKey.NumLock,
            Key.KeypadDivide => ImGuiKey.KeypadDivide,
            Key.KeypadMultiply => ImGuiKey.KeypadMultiply,
            Key.KeypadSubtract => ImGuiKey.KeypadSubtract,
            Key.KeypadAdd => ImGuiKey.KeypadAdd,
            Key.KeypadDecimal => ImGuiKey.KeypadDecimal,
            Key.KeypadEnter => ImGuiKey.KeypadEnter,
            Key.GraveAccent => ImGuiKey.GraveAccent,
            Key.Minus => ImGuiKey.Minus,
            Key.Equal => ImGuiKey.Equal,
            Key.LeftBracket => ImGuiKey.LeftBracket,
            Key.RightBracket => ImGuiKey.RightBracket,
            Key.Semicolon => ImGuiKey.Semicolon,
            Key.Apostrophe => ImGuiKey.Apostrophe,
            Key.Comma => ImGuiKey.Comma,
            Key.Period => ImGuiKey.Period,
            Key.Slash => ImGuiKey.Slash,
            Key.BackSlash => ImGuiKey.Backslash,
            _ => ImGuiKey.None
        };

        return result != ImGuiKey.None;
    }

    private void MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (TryMapMouseButton(e.MouseButton, out int result))
        {
            ImGui.GetIO().AddMouseButtonEvent(result, true);
        }
    }

    private void MouseUp(object? sender, MouseButtonEventArgs e)
    {
        if (TryMapMouseButton(e.MouseButton, out int result))
        {
            ImGui.GetIO().AddMouseButtonEvent(result, false);
        }
    }

    private void MouseMove(object? sender, MouseMoveEventArgs e)
    {
        ImGui.GetIO().AddMousePosEvent(e.PositionByScreen.X, e.PositionByScreen.Y);
    }

    private void MouseWheel(object? sender, MouseWheelEventArgs e)
    {
        ImGui.GetIO().AddMouseWheelEvent(e.ScrollWheel.X, e.ScrollWheel.Y);
    }

    private void KeyDown(object? sender, KeyEventArgs e)
    {
        if (TryMapKey(e.Key, out ImGuiKey result))
        {
            ImGui.GetIO().AddKeyEvent(result, true);
        }
    }

    private void KeyUp(object? sender, KeyEventArgs e)
    {
        if (TryMapKey(e.Key, out ImGuiKey result))
        {
            ImGui.GetIO().AddKeyEvent(result, false);
        }
    }

    private void KeyChar(object? sender, KeyCharEventArgs e)
    {
        ImGui.GetIO().AddInputCharacter(e.KeyChar);
    }

    private void Move(object? sender, MoveEventArgs e)
    {
        _viewport->PlatformRequestMove = 1;
    }

    private void Resize(object? sender, ResizeEventArgs e)
    {
        _viewport->PlatformRequestResize = 1;

        _swapchain?.Resize(e.Width, e.Height);
    }

    private void Closing(object? sender, ClosingEventArgs e)
    {
        _viewport->PlatformRequestClose = 1;
    }
}
