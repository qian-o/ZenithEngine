using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.SDL;
using Silk.NET.Windowing;

namespace Graphics.Core;

public unsafe class Window : DisposableObject
{
    private static readonly Sdl _sdl;

    private readonly IWindow _window;

    private IInputContext? inputContext;
    private IMouse? mouse;
    private IKeyboard? keyboard;
    private bool isInitialized;
    private bool isExiting;

    public event EventHandler<LoadEventArgs>? Load;
    public event EventHandler<UpdateEventArgs>? Update;
    public event EventHandler<RenderEventArgs>? Render;
    public event EventHandler<ResizeEventArgs>? Resize;
    public event EventHandler<CloseEventArgs>? Close;

    static Window()
    {
        SilkWindow.PrioritizeSdl();

        _sdl = Sdl.GetApi();
    }

    internal Window(IWindow window)
    {
        _window = window;

        Assembly();
    }

    public static Sdl Sdl => _sdl;

    public bool IsInitialized => isInitialized;

    public nint Handle => _window.Handle;

    public string Title
    {
        get => _window.Title;
        set => _window.Title = value;
    }

    public int X
    {
        get => _window.Position.X;
        set => _window.Position = new Vector2D<int>(value, Y);
    }

    public int Y
    {
        get => _window.Position.Y;
        set => _window.Position = new Vector2D<int>(X, value);
    }

    public int Width
    {
        get => _window.Size.X;
        set => _window.Size = new Vector2D<int>(value, Height);
    }

    public int Height
    {
        get => _window.Size.Y;
        set => _window.Size = new Vector2D<int>(Width, value);
    }

    public int FramebufferWidth => _window.FramebufferSize.X;

    public int FramebufferHeight => _window.FramebufferSize.Y;

    public bool Focused
    {
        get
        {
            WindowFlags flags = (WindowFlags)Sdl.GetWindowFlags((SDLWindow*)_window.Handle);

            return flags.HasFlag(WindowFlags.InputFocus);
        }
    }

    public bool Minimized
    {
        get
        {
            WindowFlags flags = (WindowFlags)Sdl.GetWindowFlags((SDLWindow*)_window.Handle);

            return flags.HasFlag(WindowFlags.Minimized);
        }
    }

    public IWindow IWindow => _window;

    public IInputContext InputContext => ThrowIfNotInitialized(inputContext);

    public IMouse Mouse => ThrowIfNotInitialized(mouse);

    public IKeyboard Keyboard => ThrowIfNotInitialized(keyboard);

    public void Run()
    {
        _window.Run();
    }

    public void Show()
    {
        _window.Initialize();

        _sdl.ShowWindow((SDLWindow*)_window.Handle);
    }

    public void Focus()
    {
        _sdl.RaiseWindow((SDLWindow*)_window.Handle);
    }

    public void Exit()
    {
        isExiting = true;
    }

    protected override void Destroy()
    {
        _window.Dispose();
    }

    private void Assembly()
    {
        _window.Load += () =>
        {
            _window.Center();

            inputContext = _window.CreateInput();
            mouse = inputContext.Mice[0];
            keyboard = inputContext.Keyboards[0];

            isInitialized = true;

            Load?.Invoke(this, new LoadEventArgs());
            Resize?.Invoke(this, new ResizeEventArgs((uint)_window.Size.X, (uint)_window.Size.Y));
        };
        _window.Update += (d) =>
        {
            if (isExiting)
            {
                _window.Close();
            }

            Update?.Invoke(this, new UpdateEventArgs((float)d, (float)_window.Time));
        };
        _window.Render += (d) =>
        {
            if (isExiting)
            {
                _window.Close();
            }

            Render?.Invoke(this, new RenderEventArgs((float)d, (float)_window.Time));
        };
        _window.FramebufferResize += (v) =>
        {
            if (isExiting)
            {
                _window.Close();
            }

            Resize?.Invoke(this, new ResizeEventArgs((uint)v.X, (uint)v.Y));
        };
        _window.Closing += () =>
        {
            Close?.Invoke(this, new CloseEventArgs());
        };
    }

    private T ThrowIfNotInitialized<T>(T? value)
    {
        if (!isInitialized)
        {
            throw new InvalidOperationException("Window not initialized yet.");
        }

        return value!;
    }

    public static Window CreateWindowByVulkan()
    {
        WindowOptions windowOptions = WindowOptions.DefaultVulkan;
        windowOptions.API = new GraphicsAPI()
        {
            API = ContextAPI.Vulkan,
            Profile = ContextProfile.Core,
            Flags = ContextFlags.ForwardCompatible,
            Version = new APIVersion(1, 3)
        };

        return new Window(SilkWindow.Create(windowOptions));
    }
}
