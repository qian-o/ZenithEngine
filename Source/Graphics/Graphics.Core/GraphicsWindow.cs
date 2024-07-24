using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Graphics.Core;

public unsafe partial class GraphicsWindow : DisposableObject
{
    private readonly IWindow _window;

    private IInputContext? inputContext;
    private IMouse? mouse;
    private IKeyboard? keyboard;
    private bool isInitialized;
    private bool isFocused;
    private bool isExiting;

    public event EventHandler<LoadEventArgs>? Load;
    public event EventHandler<UpdateEventArgs>? Update;
    public event EventHandler<RenderEventArgs>? Render;
    public event EventHandler<ResizeEventArgs>? Resize;
    public event EventHandler<MoveEventArgs>? Move;
    public event EventHandler<CloseEventArgs>? Close;

    internal GraphicsWindow(IWindow window)
    {
        _window = window;

        Assembly();
    }

    public nint Handle => _window.Handle;

    public bool IsInitialized => isInitialized;

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

    public IVkSurface? VkSurface => _window.VkSurface;

    public bool IsFocused => isFocused;

    public WindowState WindowState
    {
        get => _window.WindowState;
        set => _window.WindowState = value;
    }

    public WindowBorder WindowBorder
    {
        get => _window.WindowBorder;
        set => _window.WindowBorder = value;
    }

    public bool TopMost
    {
        get => _window.TopMost;
        set => _window.TopMost = value;
    }

    public IInputContext InputContext => ThrowIfNotInitialized(inputContext);

    public IMouse Mouse => ThrowIfNotInitialized(mouse);

    public IKeyboard Keyboard => ThrowIfNotInitialized(keyboard);

    public bool ShowInTaskbar { get; set; }

    public void Run()
    {
        _window.Run();
    }

    public void Show()
    {
        _window.Initialize();
    }

    public void Exit()
    {
        isExiting = true;
    }

    public void Focus()
    {

    }

    public void DoEvents()
    {
        _window.DoEvents();
    }

    protected override void Destroy()
    {
        _window.Dispose();
    }

    private void Assembly()
    {
        _window.Load += () =>
        {
            inputContext = _window.CreateInput();
            mouse = inputContext.Mice[0];
            keyboard = inputContext.Keyboards[0];

            isInitialized = true;

            Load?.Invoke(this, new LoadEventArgs());
            Resize?.Invoke(this, new ResizeEventArgs((uint)_window.Size.X, (uint)_window.Size.Y));
        };
        _window.FocusChanged += (b) =>
        {
            if (isExiting)
            {
                _window.Close();
            }

            isFocused = b;
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
        _window.Move += (v) =>
        {
            if (isExiting)
            {
                _window.Close();
            }

            Move?.Invoke(this, new MoveEventArgs(v.X, v.Y));
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

    public static GraphicsWindow CreateWindowByVulkan()
    {
        WindowOptions windowOptions = WindowOptions.DefaultVulkan;
        windowOptions.API = new GraphicsAPI()
        {
            API = ContextAPI.Vulkan,
            Profile = ContextProfile.Core,
            Flags = ContextFlags.ForwardCompatible,
            Version = new APIVersion(1, 3)
        };

        return new GraphicsWindow(Window.Create(windowOptions));
    }
}