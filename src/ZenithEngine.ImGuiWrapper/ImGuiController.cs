using Hexa.NET.ImGui;
using Silk.NET.Maths;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Events;
using ZenithEngine.Common.Graphics;
using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.ImGuiWrapper;

public unsafe class ImGuiController : DisposableObject
{
    public const ImGuiMouseButton MouseButtonX1 = (ImGuiMouseButton)3;
    public const ImGuiMouseButton MouseButtonX2 = (ImGuiMouseButton)4;

    public ImGuiContextPtr ImGuiContext;

    private bool frameBegun;

    public ImGuiController(GraphicsContext graphicsContext,
                           OutputDesc outputDesc,
                           IInputController inputController,
                           ColorSpaceHandling colorSpaceHandling = ColorSpaceHandling.Legacy,
                           ImGuiFontConfig? fontConfig = null,
                           Action<ImGuiIOPtr>? ioConfig = null)
    {
        ImGui.SetCurrentContext(ImGuiContext = ImGui.CreateContext());

        Renderer = new(graphicsContext, outputDesc, colorSpaceHandling);
        InputController = inputController;

        Initialize(fontConfig, ioConfig);
    }

    internal ImGuiRenderer Renderer { get; }

    internal IInputController InputController { get; }

    public void Update(double deltaSeconds, Vector2D<uint> size)
    {
        if (frameBegun)
        {
            ImGui.Render();
        }

        ImGui.SetCurrentContext(ImGuiContext);

        InputController.Cursor = ImGui.GetMouseCursor() switch
        {
            ImGuiMouseCursor.TextInput => Cursor.TextInput,
            ImGuiMouseCursor.ResizeAll => Cursor.ResizeAll,
            ImGuiMouseCursor.ResizeNs => Cursor.ResizeNS,
            ImGuiMouseCursor.ResizeEw => Cursor.ResizeWE,
            ImGuiMouseCursor.ResizeNesw => Cursor.ResizeNESW,
            ImGuiMouseCursor.ResizeNwse => Cursor.ResizeNWSE,
            ImGuiMouseCursor.Hand => Cursor.Hand,
            ImGuiMouseCursor.NotAllowed => Cursor.NotAllowed,
            _ => Cursor.Arrow
        };

        ImGuiIOPtr io = ImGui.GetIO();

        io.DeltaTime = (float)deltaSeconds;
        io.DisplaySize = size.As<float>().ToSystem();

        ImGui.NewFrame();

        ImGui.DockSpaceOverViewport();

        frameBegun = true;
    }

    public void PrepareResources(CommandBuffer commandBuffer)
    {
        Renderer.PrepareResources(commandBuffer);
    }

    public void Render(CommandBuffer commandBuffer)
    {
        if (frameBegun)
        {
            ImGui.Render();

            Renderer.Render(commandBuffer, ImGui.GetDrawData());

            frameBegun = false;
        }
    }

    public ulong GetBinding(TextureView textureView)
    {
        return Renderer.GetBinding(textureView);
    }

    public ulong GetBinding(Texture texture)
    {
        return Renderer.GetBinding(texture);
    }

    public void RemoveBinding(TextureView textureView)
    {
        Renderer.RemoveBinding(textureView);
    }

    public void RemoveBinding(Texture texture)
    {
        Renderer.RemoveBinding(texture);
    }

    protected override void Destroy()
    {
        InputController.KeyUp -= KeyUp;
        InputController.KeyDown -= KeyDown;
        InputController.KeyChar -= KeyChar;
        InputController.MouseUp -= MouseUp;
        InputController.MouseDown -= MouseDown;
        InputController.MouseMove -= MouseMove;
        InputController.MouseWheel -= MouseWheel;

        Renderer.Dispose();

        ImGui.SetCurrentContext(null);

        ImGui.DestroyContext(ImGuiContext);
    }

    private void Initialize(ImGuiFontConfig? fontConfig, Action<ImGuiIOPtr>? ioConfig)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        if (fontConfig is not null)
        {
            io.Fonts.Clear();

            io.Fonts.AddFontFromFileTTF(fontConfig.Value.Font,
                                        (int)fontConfig.Value.Size,
                                        null,
                                        (uint*)fontConfig.Value.GlyphRange(io));
        }

        ioConfig?.Invoke(io);

        Renderer.CreateFontDeviceTexture();

        InputController.KeyUp += KeyUp;
        InputController.KeyDown += KeyDown;
        InputController.KeyChar += KeyChar;
        InputController.MouseUp += MouseUp;
        InputController.MouseDown += MouseDown;
        InputController.MouseMove += MouseMove;
        InputController.MouseWheel += MouseWheel;
    }

    private void KeyUp(object? sender, KeyEventArgs e)
    {
        if (TryMapKey(e.Key, out ImGuiKey result))
        {
            ImGui.GetIO().AddKeyEvent(result, false);
        }
    }

    private void KeyDown(object? sender, KeyEventArgs e)
    {
        if (TryMapKey(e.Key, out ImGuiKey result))
        {
            ImGui.GetIO().AddKeyEvent(result, true);
        }
    }

    private void KeyChar(object? sender, ValueEventArgs<char> e)
    {
        ImGui.GetIO().AddInputCharacter(e.Value);
    }

    private void MouseUp(object? sender, MouseButtonEventArgs e)
    {
        if (TryMapMouseButton(e.Button, out ImGuiMouseButton result))
        {
            ImGui.GetIO().AddMouseButtonEvent((int)result, false);
        }
    }

    private void MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (TryMapMouseButton(e.Button, out ImGuiMouseButton result))
        {
            ImGui.GetIO().AddMouseButtonEvent((int)result, true);
        }
    }

    private void MouseMove(object? sender, ValueEventArgs<Vector2D<int>> e)
    {
        ImGui.GetIO().AddMousePosEvent(e.Value.X, e.Value.Y);
    }

    private void MouseWheel(object? sender, ValueEventArgs<Vector2D<int>> e)
    {
        ImGui.GetIO().AddMouseWheelEvent(e.Value.X, e.Value.Y);
    }

    private static bool TryMapKey(Key key, out ImGuiKey result)
    {
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

        return result is not ImGuiKey.None;


        static ImGuiKey KeyToImGuiKeyShortcut(Key keyToConvert, Key startKey1, ImGuiKey startKey2)
        {
            int changeFromStart1 = (int)keyToConvert - (int)startKey1;
            return startKey2 + changeFromStart1;
        }
    }

    private static bool TryMapMouseButton(MouseButton button, out ImGuiMouseButton result)
    {
        result = button switch
        {
            MouseButton.Left => ImGuiMouseButton.Left,
            MouseButton.Right => ImGuiMouseButton.Right,
            MouseButton.Middle => ImGuiMouseButton.Middle,
            MouseButton.Button4 => MouseButtonX1,
            MouseButton.Button5 => MouseButtonX2,
            _ => ImGuiMouseButton.Count
        };

        return result is not ImGuiMouseButton.Count;
    }
}
