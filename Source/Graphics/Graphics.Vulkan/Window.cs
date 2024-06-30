using Graphics.Core;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Graphics.Vulkan;

public class Window : DisposableObject
{
    private readonly IWindow _window;

    private IInputContext? inputContext;
    private IMouse? mouse;
    private IKeyboard? keyboard;
    private bool isInitialized;

    public Window()
    {
        WindowOptions windowOptions = WindowOptions.DefaultVulkan;
        windowOptions.API = new GraphicsAPI()
        {
            API = ContextAPI.Vulkan,
            Profile = ContextProfile.Core,
            Flags = ContextFlags.ForwardCompatible,
            Version = new APIVersion(1, 3)
        };

        _window = SilkWindow.Create(windowOptions);
    }

    public event EventHandler<LoadEventArgs>? Load;

    public event EventHandler<UpdateEventArgs>? Update;

    public event EventHandler<RenderEventArgs>? Render;

    public event EventHandler<ResizeEventArgs>? Resize;

    public event EventHandler<CloseEventArgs>? Close;

    public string Title
    {
        get => _window.Title;
        set => _window.Title = value;
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

    public IWindow IWindow => _window;

    public IMouse Mouse => ThrowIfNotInitialized(mouse);

    public IKeyboard Keyboard => ThrowIfNotInitialized(keyboard);

    public void Run()
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
        _window.Update += (d) => Update?.Invoke(this, new UpdateEventArgs((float)d));
        _window.Render += (d) => Render?.Invoke(this, new RenderEventArgs((float)d));
        _window.Resize += (v) => Resize?.Invoke(this, new ResizeEventArgs((uint)v.X, (uint)v.Y));
        _window.Closing += () => Close?.Invoke(this, new CloseEventArgs());

        _window.Run();
    }

    protected override void Destroy()
    {
        _window.Dispose();
    }

    private T ThrowIfNotInitialized<T>(T? value)
    {
        if (!isInitialized)
        {
            throw new InvalidOperationException("Window not initialized yet.");
        }

        return value!;
    }
}
