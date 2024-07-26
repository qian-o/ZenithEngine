using System.Numerics;
using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.SDL;
using Silk.NET.Windowing;

namespace Graphics.Core;

public unsafe partial class GraphicsWindow : DisposableObject
{
    private static readonly Sdl _sdl;

    private readonly IWindow _window;
    private readonly IInputContext _inputContext;
    private readonly IMouse _mouse;
    private readonly IKeyboard _keyboard;

    static GraphicsWindow()
    {
        _sdl = Sdl.GetApi();

        SilkWindow.PrioritizeSdl();
    }

    internal GraphicsWindow(IWindow window, IInputContext inputContext)
    {
        _window = window;
        _inputContext = inputContext;
        _mouse = _inputContext.Mice[0];
        _keyboard = _inputContext.Keyboards[0];

        Assembly();
    }

    public nint Handle => _window.Handle;

    public IVkSurface? VkSurface => _window.VkSurface;

    protected override void Destroy()
    {
        _window.Close();

        _inputContext.Dispose();
        _window.Dispose();
    }

    private void Assembly()
    {
        AssemblyStatusEvent();
        AssemblyMouseEvent();
        AssemblyKeyboardEvent();
    }

    public static GraphicsWindow CreateWindowByVulkan()
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

        return new GraphicsWindow(window, window.CreateInput());
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

        float dpi;
        _sdl.GetDisplayDPI(index, &dpi, null, null);

        return new Display(index,
                           name,
                           new Vector2(main.Origin.X, main.Origin.Y),
                           new Vector2(main.Size.X, main.Size.Y),
                           new Vector2(work.Origin.X, work.Origin.Y),
                           new Vector2(work.Size.X, work.Size.Y),
                           dpi / 96.0f);
    }
}