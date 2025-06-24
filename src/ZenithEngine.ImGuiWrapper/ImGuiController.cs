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
    public ImGuiContextPtr ImGuiContext;

    private bool frameBegun;

    public ImGuiController(IInput input,
                           GraphicsContext graphicsContext,
                           OutputDesc outputDesc,
                           ColorSpaceHandling colorSpaceHandling = ColorSpaceHandling.Legacy,
                           ImGuiFontConfig? fontConfig = null,
                           Action<ImGuiIOPtr>? ioConfig = null)
    {
        ImGui.SetCurrentContext(ImGuiContext = ImGui.CreateContext());

        Input = input;
        Renderer = new(graphicsContext, outputDesc, colorSpaceHandling);

        Initialize(fontConfig, ioConfig);
    }

    internal IInput Input { get; }

    internal ImGuiRenderer Renderer { get; }

    public void Update(double deltaSeconds, Vector2D<uint> size)
    {
        if (frameBegun)
        {
            ImGui.Render();
        }

        ImGui.SetCurrentContext(ImGuiContext);

        Input.Cursor = ImGui.GetMouseCursor() switch
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

    public ulong GetBinding(Texture texture)
    {
        return Renderer.GetBinding(texture);
    }

    public void RemoveBinding(Texture texture)
    {
        Renderer.RemoveBinding(texture);
    }

    protected override void Destroy()
    {
        Input.KeyUp -= KeyUp;
        Input.KeyDown -= KeyDown;
        Input.KeyChar -= KeyChar;
        Input.MouseUp -= MouseUp;
        Input.MouseDown -= MouseDown;
        Input.MouseMove -= MouseMove;
        Input.MouseWheel -= MouseWheel;
        Input.GamepadButtonUp -= GamepadButtonUp;
        Input.GamepadButtonDown -= GamepadButtonDown;
        Input.GamepadAxisMotion -= GamepadAxisMotion;

        Renderer.Dispose();

        ImGui.SetCurrentContext(null);

        ImGui.DestroyContext(ImGuiContext);
    }

    private void Initialize(ImGuiFontConfig? fontConfig, Action<ImGuiIOPtr>? ioConfig)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;

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

        Input.KeyUp += KeyUp;
        Input.KeyDown += KeyDown;
        Input.KeyChar += KeyChar;
        Input.MouseUp += MouseUp;
        Input.MouseDown += MouseDown;
        Input.MouseMove += MouseMove;
        Input.MouseWheel += MouseWheel;
        Input.GamepadButtonUp += GamepadButtonUp;
        Input.GamepadButtonDown += GamepadButtonDown;
        Input.GamepadAxisMotion += GamepadAxisMotion;
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

    private void GamepadButtonDown(object? sender, GamepadButtonEventArgs e)
    {
        if (TryMapGamepadButton(e.Button, out ImGuiKey result))
        {
            ImGui.GetIO().AddKeyEvent(result, true);
        }
    }

    private void GamepadButtonUp(object? sender, GamepadButtonEventArgs e)
    {
        if (TryMapGamepadButton(e.Button, out ImGuiKey result))
        {
            ImGui.GetIO().AddKeyEvent(result, false);
        }
    }

    private void GamepadAxisMotion(object? sender, GamepadAxisEventArgs e)
    {
        var io = ImGui.GetIO();

        switch (e.Axis)
        {
            case GamepadAxis.LeftX:
                io.AddKeyAnalogEvent(ImGuiKey.GamepadLStickLeft, e.Value < -0.1f, Math.Abs(Math.Min(e.Value, 0.0f)));
                io.AddKeyAnalogEvent(ImGuiKey.GamepadLStickRight, e.Value > 0.1f, Math.Max(e.Value, 0.0f));
                break;
            case GamepadAxis.LeftY:
                io.AddKeyAnalogEvent(ImGuiKey.GamepadLStickUp, e.Value < -0.1f, Math.Abs(Math.Min(e.Value, 0.0f)));
                io.AddKeyAnalogEvent(ImGuiKey.GamepadLStickDown, e.Value > 0.1f, Math.Max(e.Value, 0.0f));
                break;
            case GamepadAxis.RightX:
                io.AddKeyAnalogEvent(ImGuiKey.GamepadRStickLeft, e.Value < -0.1f, Math.Abs(Math.Min(e.Value, 0.0f)));
                io.AddKeyAnalogEvent(ImGuiKey.GamepadRStickRight, e.Value > 0.1f, Math.Max(e.Value, 0.0f));
                break;
            case GamepadAxis.RightY:
                io.AddKeyAnalogEvent(ImGuiKey.GamepadRStickUp, e.Value < -0.1f, Math.Abs(Math.Min(e.Value, 0.0f)));
                io.AddKeyAnalogEvent(ImGuiKey.GamepadRStickDown, e.Value > 0.1f, Math.Max(e.Value, 0.0f));
                break;
            case GamepadAxis.TriggerLeft:
                io.AddKeyAnalogEvent(ImGuiKey.GamepadL2, e.Value > 0.1f, e.Value);
                break;
            case GamepadAxis.TriggerRight:
                io.AddKeyAnalogEvent(ImGuiKey.GamepadR2, e.Value > 0.1f, e.Value);
                break;
        }
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
            MouseButton.Button4 => ImGuiMouseButton.Right | ImGuiMouseButton.Middle,
            MouseButton.Button5 => (ImGuiMouseButton)4,
            _ => ImGuiMouseButton.Count
        };

        return result is not ImGuiMouseButton.Count;
    }

    private static bool TryMapGamepadButton(GamepadButton button, out ImGuiKey result)
    {
        result = button switch
        {
            GamepadButton.A => ImGuiKey.GamepadFaceDown,
            GamepadButton.B => ImGuiKey.GamepadFaceRight,
            GamepadButton.X => ImGuiKey.GamepadFaceLeft,
            GamepadButton.Y => ImGuiKey.GamepadFaceUp,
            GamepadButton.Back => ImGuiKey.GamepadBack,
            GamepadButton.Start => ImGuiKey.GamepadStart,
            GamepadButton.LeftStick => ImGuiKey.GamepadL3,
            GamepadButton.RightStick => ImGuiKey.GamepadR3,
            GamepadButton.LeftShoulder => ImGuiKey.GamepadL1,
            GamepadButton.RightShoulder => ImGuiKey.GamepadR1,
            GamepadButton.DPadUp => ImGuiKey.GamepadDpadUp,
            GamepadButton.DPadDown => ImGuiKey.GamepadDpadDown,
            GamepadButton.DPadLeft => ImGuiKey.GamepadDpadLeft,
            GamepadButton.DPadRight => ImGuiKey.GamepadDpadRight,
            _ => ImGuiKey.COUNT
        };

        return result is not ImGuiKey.COUNT;
    }
}
