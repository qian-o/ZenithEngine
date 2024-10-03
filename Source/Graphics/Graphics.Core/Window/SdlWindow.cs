using System.Numerics;
using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.SDL;
using Silk.NET.Windowing;

namespace Graphics.Core;

public unsafe partial class SdlWindow : DisposableObject
{
    private static readonly Sdl _sdl;
    private static readonly Dictionary<MouseButton, int> _buttonMasks;

    private readonly IWindow _window;
    private readonly IInputContext _inputContext;
    private readonly IMouse _mouse;
    private readonly IKeyboard _keyboard;

    static SdlWindow()
    {
        _sdl = Sdl.GetApi();
        _buttonMasks = [];
        _buttonMasks[MouseButton.Left] = 1;
        _buttonMasks[MouseButton.Middle] = 2;
        _buttonMasks[MouseButton.Right] = 3;
        _buttonMasks[MouseButton.Button4] = 4;
        _buttonMasks[MouseButton.Button5] = 5;
    }

    internal SdlWindow(IWindow window, IInputContext inputContext)
    {
        _window = window;
        _inputContext = inputContext;
        _mouse = _inputContext.Mice[0];
        _keyboard = _inputContext.Keyboards[0];

        Initialize();
    }

    public nint Handle => _window.Handle;

    public IVkSurface? VkSurface => _window.VkSurface;

    protected override void Destroy()
    {
        _window.Close();

        _inputContext.Dispose();
        _window.Dispose();
    }

    private void Initialize()
    {
        AssemblyStatusEvent();
        AssemblyMouseEvent();
        AssemblyKeyboardEvent();
    }

    public static SdlWindow CreateWindowByVulkan()
    {
        WindowOptions windowOptions = WindowOptions.DefaultVulkan;
        windowOptions.IsVisible = false;
        windowOptions.API = new GraphicsAPI()
        {
            API = ContextAPI.Vulkan,
            Profile = ContextProfile.Core,
            Flags = ContextFlags.ForwardCompatible,
            Version = new APIVersion(1, 3)
        };

        IWindow window = SilkWindow.Create(windowOptions);
        window.Initialize();

        return new SdlWindow(window, window.CreateInput());
    }

    public static bool IsMouseFocusOnWindow()
    {
        return _sdl.GetMouseFocus() != null;
    }

    public static MouseButton[] GetGlobalMouseState(out Vector2 position)
    {
        int x, y;
        uint mask = _sdl.GetGlobalMouseState(&x, &y);

        position = new Vector2(x, y);

        List<MouseButton> buttons = [];
        foreach (KeyValuePair<MouseButton, int> buttonMask in _buttonMasks)
        {
            if ((mask & (1 << (buttonMask.Value - 1))) != 0)
            {
                buttons.Add(buttonMask.Key);
            }
        }

        return [.. buttons];
    }

    public static Cursor* CreateCursor(SystemCursor systemCursor)
    {
        return _sdl.CreateSystemCursor(systemCursor);
    }

    public static void SetCursor(Cursor* cursor)
    {
        _sdl.SetCursor(cursor);
    }

    public static void FreeCursor(Cursor* cursor)
    {
        _sdl.FreeCursor(cursor);
    }

    public static void SetTextIputRect(int x, int y, int w, int h)
    {
        Rectangle<int> rect = new(x, y, w, h);

        _sdl.SetTextInputRect(&rect);
    }

    public static int GetDisplayCount()
    {
        return _sdl.GetNumVideoDisplays();
    }

    public static Display GetDisplay(int index)
    {
        string name = _sdl.GetDisplayNameS(index);

        Rectangle<int> main;
        _sdl.GetDisplayBounds(index, &main);

        Rectangle<int> work;
        _sdl.GetDisplayUsableBounds(index, &work);

        float ddpi;
        _sdl.GetDisplayDPI(index, &ddpi, null, null);

        return new Display(index,
                           name,
                           new Vector2(main.Origin.X, main.Origin.Y),
                           new Vector2(main.Size.X, main.Size.Y),
                           new Vector2(work.Origin.X, work.Origin.Y),
                           new Vector2(work.Size.X, work.Size.Y),
                           ddpi == 0 ? 1.0f : ddpi / 96.0f);
    }
}