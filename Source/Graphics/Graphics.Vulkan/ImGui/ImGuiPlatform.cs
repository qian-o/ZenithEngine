using System.Numerics;
using Graphics.Core;
using Graphics.Core.Window;
using Graphics.Vulkan.Descriptions;
using Hexa.NET.ImGui;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Graphics.Vulkan.ImGui;

public unsafe class ImGuiPlatform : DisposableObject
{
    private readonly SdlWindow _window;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly bool _isExternalPlatform;

    internal ImGuiPlatform(ImGuiViewport* viewport, SdlWindow window, GraphicsDevice graphicsDevice)
    {
        Viewport = viewport;

        _window = window;
        _graphicsDevice = graphicsDevice;
        _isExternalPlatform = true;

        Initialize();
    }

    internal ImGuiPlatform(ImGuiViewport* viewport, GraphicsDevice graphicsDevice)
    {
        Viewport = viewport;

        _window = SdlWindow.CreateWindowByVulkan();
        _graphicsDevice = graphicsDevice;
        _isExternalPlatform = false;

        Initialize();
    }

    public ImGuiViewport* Viewport { get; }

    public Swapchain? Swapchain { get; private set; }

    public string Title { get => _window.Title; set => _window.Title = value; }

    public Vector2 Position { get => _window.Position; set => _window.Position = value; }

    public Vector2 Size { get => _window.Size; set => _window.Size = value; }

    public float DpiScale => _window.DpiScale;

    public byte IsFocused => _window.IsFocused ? (byte)1 : (byte)0;

    public byte IsMinimized => _window.WindowState == WindowState.Minimized ? (byte)1 : (byte)0;

    public float Alpha { get => _window.Opacity; set => _window.Opacity = value; }

    public void Show()
    {
        if (_isExternalPlatform)
        {
            return;
        }

        _window.Show();
    }

    public void Focus()
    {
        _window.Focus();
    }

    public void Update()
    {
        if (_isExternalPlatform)
        {
            return;
        }

        _window.PollEvents();

        if (Swapchain!.Width != _window.FramebufferSize.X || Swapchain!.Height != _window.FramebufferSize.Y)
        {
            Swapchain.Resize((uint)_window.FramebufferSize.X, (uint)_window.FramebufferSize.Y);
        }
    }

    public void SwapBuffers()
    {
        if (_isExternalPlatform)
        {
            return;
        }

        _graphicsDevice.SwapBuffers(Swapchain!);
    }

    protected override void Destroy()
    {
        Swapchain?.Dispose();

        Unregister();

        if (!_isExternalPlatform)
        {
            _window.Dispose();
        }
    }

    private void Initialize()
    {
        Viewport->PlatformHandle = (void*)_window.Handle;

        if (!_isExternalPlatform)
        {
            if (Viewport->Flags.HasFlag(ImGuiViewportFlags.NoTaskBarIcon))
            {
                _window.ShowInTaskbar = false;
            }

            if (Viewport->Flags.HasFlag(ImGuiViewportFlags.NoDecoration))
            {
                _window.WindowBorder = WindowBorder.Hidden;
            }

            if (Viewport->Flags.HasFlag(ImGuiViewportFlags.TopMost))
            {
                _window.TopMost = true;
            }

            SwapchainDescription swapchainDescription = new()
            {
                Target = _window.VkSurface!,
                Width = (uint)_window.FramebufferSize.X,
                Height = (uint)_window.FramebufferSize.Y,
                DepthFormat = _graphicsDevice.GetBestDepthFormat()
            };

            Swapchain = _graphicsDevice.Factory.CreateSwapchain(in swapchainDescription);
        }

        Register();
    }

    private void Register()
    {
        _window.MouseWheel += MouseWheel;
        _window.KeyDown += KeyDown;
        _window.KeyUp += KeyUp;
        _window.KeyChar += KeyChar;
        _window.Move += Move;
        _window.Resize += Resize;
        _window.Closing += Closing;
    }

    private void Unregister()
    {
        _window.MouseWheel -= MouseWheel;
        _window.KeyDown -= KeyDown;
        _window.KeyUp -= KeyUp;
        _window.KeyChar -= KeyChar;
        _window.Move -= Move;
        _window.Resize -= Resize;
        _window.Closing -= Closing;
    }

    private void MouseWheel(object? sender, MouseWheelEventArgs e)
    {
        DearImGui.GetIO().AddMouseWheelEvent(e.ScrollWheel.X, e.ScrollWheel.Y);
    }

    private void KeyDown(object? sender, KeyEventArgs e)
    {
        if (TryMapKey(e.Key, out ImGuiKey result))
        {
            DearImGui.GetIO().AddKeyEvent(result, true);
        }
    }

    private void KeyUp(object? sender, KeyEventArgs e)
    {
        if (TryMapKey(e.Key, out ImGuiKey result))
        {
            DearImGui.GetIO().AddKeyEvent(result, false);
        }
    }

    private void KeyChar(object? sender, KeyCharEventArgs e)
    {
        DearImGui.GetIO().AddInputCharacter(e.KeyChar);
    }

    private void Move(object? sender, MoveEventArgs e)
    {
        Viewport->PlatformRequestMove = 1;
    }

    private void Resize(object? sender, ResizeEventArgs e)
    {
        Viewport->PlatformRequestResize = 1;

        Swapchain?.Resize(e.Width, e.Height);
    }

    private void Closing(object? sender, ClosingEventArgs e)
    {
        Viewport->PlatformRequestClose = 1;
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
}
