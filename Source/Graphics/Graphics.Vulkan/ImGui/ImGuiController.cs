using Graphics.Core;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Silk.NET.Input;

namespace Graphics.Vulkan;

public unsafe class ImGuiController : DisposableObject
{
    private static readonly Key[] _keyEnumArr = (Key[])Enum.GetValues(typeof(Key));

    private readonly GraphicsWindow _graphicsWindow;
    private readonly ImGuiContextPtr _imGuiContext;
    private readonly ImGuiRenderer _imGuiRenderer;
    private readonly List<char> _pressedChars = [];

    #region Constructors
    public ImGuiController(GraphicsWindow graphicsWindow,
                           GraphicsDevice graphicsDevice,
                           ColorSpaceHandling colorSpaceHandling,
                           ImGuiFontConfig? imGuiFontConfig,
                           Action? onConfigureIO)
    {
        _graphicsWindow = graphicsWindow;
        _imGuiContext = ImGui.CreateContext();
        _imGuiRenderer = new ImGuiRenderer(graphicsDevice, colorSpaceHandling);

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
        ImGui.SetCurrentContext(_imGuiContext);
        ImGuizmo.SetImGuiContext(_imGuiContext);

        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput();

        ImGui.NewFrame();
        ImGuizmo.BeginFrame();
    }

    public void Render(CommandList commandList)
    {
        ImGui.Render();
        _imGuiRenderer.RenderImDrawData(commandList, ImGui.GetDrawData());

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
        _graphicsWindow.KeyChar -= GraphicsWindow_KeyChar;

        ImGui.DestroyPlatformWindows();

        _imGuiRenderer.Dispose();

        ImGui.DestroyContext(_imGuiContext);
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

    private void UpdateImGuiInput()
    {
        ImGuiIOPtr io = ImGui.GetIO();

        io.AddMouseButtonEvent(0, _graphicsWindow.IsButtonPressed(MouseButton.Left));
        io.AddMouseButtonEvent(1, _graphicsWindow.IsButtonPressed(MouseButton.Right));
        io.AddMouseButtonEvent(2, _graphicsWindow.IsButtonPressed(MouseButton.Middle));
        io.AddMouseButtonEvent(3, _graphicsWindow.IsButtonPressed(MouseButton.Button4));
        io.AddMouseButtonEvent(4, _graphicsWindow.IsButtonPressed(MouseButton.Button5));
        io.AddMouseWheelEvent(_graphicsWindow.ScrollWheel.X, _graphicsWindow.ScrollWheel.Y);
        io.AddMousePosEvent(_graphicsWindow.MousePositionByWindow.X, _graphicsWindow.MousePositionByWindow.Y);

        foreach (char pressedChar in _pressedChars)
        {
            io.AddInputCharacter(pressedChar);
        }

        foreach (Key key in _keyEnumArr)
        {
            if (key != Key.Unknown && TryMapKey(key, out ImGuiKey imGuiKey))
            {
                io.AddKeyEvent(imGuiKey, _graphicsWindow.IsKeyPressed(key));
            }
        }

        _pressedChars.Clear();
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
        // io.BackendFlags |= ImGuiBackendFlags.PlatformHasViewports;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasViewports;

        _graphicsWindow.KeyChar += GraphicsWindow_KeyChar;
    }

    private void GraphicsWindow_KeyChar(object? sender, KeyCharEventArgs e)
    {
        _pressedChars.Add(e.KeyChar);
    }
}
