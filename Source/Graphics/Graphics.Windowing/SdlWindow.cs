using System.Diagnostics;
using System.Text;
using Graphics.Core.Helpers;
using Graphics.Windowing.Enums;
using Graphics.Windowing.Events;
using Graphics.Windowing.Interfaces;
using Graphics.Windowing.Structs;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace Graphics.Windowing;

public unsafe class SdlWindow : IWindow
{
    public event EventHandler<EventArgs>? Loaded;

    public event EventHandler<EventArgs>? Unloaded;

    public event EventHandler<ValueEventArgs<WindowState>>? StateChanged;

    public event EventHandler<ValueEventArgs<Vector2D<int>>>? PositionChanged;

    public event EventHandler<ValueEventArgs<Vector2D<int>>>? SizeChanged;

    public event EventHandler<KeyEventArgs>? KeyDown;

    public event EventHandler<KeyEventArgs>? KeyUp;

    public event EventHandler<ValueEventArgs<char>>? KeyChar;

    public event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseMove;

    public event EventHandler<MouseButtonEventArgs>? MouseDown;

    public event EventHandler<MouseButtonEventArgs>? MouseUp;

    public event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseWheel;

    public event EventHandler<MouseButtonEventArgs>? Click;

    public event EventHandler<MouseButtonEventArgs>? DoubleClick;

    public event EventHandler<TimeEventArgs>? Update;

    public event EventHandler<TimeEventArgs>? Render;

    private readonly Stopwatch updateStopwatch = new();
    private readonly Stopwatch renderStopwatch = new();
    private readonly Stopwatch lifetimeStopwatch = new();

    private Window* window;
    private bool isLoopRunning;
    private SdlVkSurface? vkSurface;

    private string title = "SdlWindow";
    private WindowState state = WindowState.Normal;
    private WindowBorder border = WindowBorder.Resizable;
    private Vector2D<int> minimumSize;
    private Vector2D<int> maximumSize;
    private Vector2D<int> position = new(50, 50);
    private Vector2D<int> size = new(800, 600);
    private bool isVisible = true;
    private bool topMost;
    private bool showInTaskbar = true;
    private float opacity = 1.0f;
    private double updatePeriod;
    private double renderPeriod;

    public string Title
    {
        get
        {
            return title;
        }
        set
        {
            title = value;

            if (IsCreated)
            {
                SdlManager.Sdl.SetWindowTitle(window, Encoding.UTF8.GetBytes(value).AsPointer());
            }
        }
    }

    public WindowState State
    {
        get
        {
            if (IsCreated)
            {
                WindowFlags flags = (WindowFlags)SdlManager.Sdl.GetWindowFlags(window);

                if (flags.HasFlag(WindowFlags.Minimized))
                {
                    return state = WindowState.Minimized;
                }
                else if (flags.HasFlag(WindowFlags.Maximized))
                {
                    return state = WindowState.Maximized;
                }
                else if (flags.HasFlag(WindowFlags.Fullscreen))
                {
                    return state = WindowState.Fullscreen;
                }
                else
                {
                    return state = WindowState.Normal;
                }
            }

            return state;
        }
        set
        {
            state = value;

            if (IsCreated)
            {
                switch (value)
                {
                    case WindowState.Normal:
                        SdlManager.Sdl.RestoreWindow(window);
                        break;
                    case WindowState.Minimized:
                        SdlManager.Sdl.MinimizeWindow(window);
                        break;
                    case WindowState.Maximized:
                        SdlManager.Sdl.MaximizeWindow(window);
                        break;
                    case WindowState.Fullscreen:
                        SdlManager.Sdl.SetWindowFullscreen(window, (uint)WindowFlags.Fullscreen);
                        break;
                }
            }
        }
    }

    public WindowBorder Border
    {
        get
        {
            return border;
        }
        set
        {
            border = value;

            if (IsCreated)
            {
                switch (value)
                {
                    case WindowBorder.Resizable:
                        SdlManager.Sdl.SetWindowBordered(window, SdlBool.True);
                        SdlManager.Sdl.SetWindowResizable(window, SdlBool.True);
                        break;
                    case WindowBorder.Fixed:
                        SdlManager.Sdl.SetWindowBordered(window, SdlBool.True);
                        SdlManager.Sdl.SetWindowResizable(window, SdlBool.False);
                        break;
                    case WindowBorder.Hidden:
                        SdlManager.Sdl.SetWindowBordered(window, SdlBool.False);
                        SdlManager.Sdl.SetWindowBordered(window, SdlBool.False);
                        break;
                }
            }
        }
    }

    public Vector2D<int> MinimumSize
    {
        get
        {
            return minimumSize;
        }
        set
        {
            minimumSize = value;

            if (IsCreated)
            {
                SdlManager.Sdl.SetWindowMinimumSize(window, value.X, value.Y);
            }
        }
    }

    public Vector2D<int> MaximumSize
    {
        get
        {
            return maximumSize;
        }
        set
        {
            maximumSize = value;

            if (IsCreated)
            {
                SdlManager.Sdl.SetWindowMaximumSize(window, value.X, value.Y);
            }
        }
    }

    public Vector2D<int> Position
    {
        get
        {
            if (IsCreated)
            {
                int x, y;
                SdlManager.Sdl.GetWindowPosition(window, &x, &y);

                return position = new Vector2D<int>(x, y);
            }

            return position;
        }
        set
        {
            position = value;

            if (IsCreated)
            {
                SdlManager.Sdl.SetWindowPosition(window, value.X, value.Y);
            }
        }
    }

    public Vector2D<int> Size
    {
        get
        {
            if (IsCreated)
            {
                int width, height;
                SdlManager.Sdl.GetWindowSize(window, &width, &height);

                return size = new Vector2D<int>(width, height);
            }

            return size;
        }
        set
        {
            size = value;

            if (IsCreated)
            {
                SdlManager.Sdl.SetWindowSize(window, value.X, value.Y);
            }
        }
    }

    public bool IsVisible
    {
        get
        {
            return isVisible;
        }
        set
        {
            isVisible = value;

            if (IsCreated)
            {
                if (value)
                {
                    SdlManager.Sdl.ShowWindow(window);
                }
                else
                {
                    SdlManager.Sdl.HideWindow(window);
                }
            }
        }
    }

    public bool TopMost
    {
        get
        {
            return topMost;
        }
        set
        {
            topMost = value;

            if (IsCreated)
            {
                if (value)
                {
                    SdlManager.Sdl.SetWindowAlwaysOnTop(window, SdlBool.True);
                }
                else
                {
                    SdlManager.Sdl.SetWindowAlwaysOnTop(window, SdlBool.False);
                }
            }
        }
    }

    public bool ShowInTaskbar
    {
        get
        {
            return showInTaskbar;
        }
        set
        {
            showInTaskbar = value;

            if (IsCreated)
            {
                // SDL does not support modifying this property after the window is created.
            }
        }
    }

    public float Opacity
    {
        get
        {
            return opacity;
        }
        set
        {
            opacity = value;

            if (IsCreated)
            {
                SdlManager.Sdl.SetWindowOpacity(window, value);
            }
        }
    }

    public double UpdatePerSecond
    {
        get
        {
            return updatePeriod <= double.Epsilon ? 0.0 : 1.0 / updatePeriod;
        }
        set
        {
            updatePeriod = value <= double.Epsilon ? 0.0 : 1.0 / value;
        }
    }

    public double RenderPerSecond
    {
        get
        {
            return renderPeriod <= double.Epsilon ? 0.0 : 1.0 / renderPeriod;
        }
        set
        {
            renderPeriod = value <= double.Epsilon ? 0.0 : 1.0 / value;
        }
    }

    public bool IsCreated
    {
        get
        {
            return window != null;
        }
    }

    public nint Handle
    {
        get
        {
            return (nint)window;
        }
    }

    public float DpiScale
    {
        get
        {
            int displayIndex = SdlManager.Sdl.GetWindowDisplayIndex(window);

            float ddpi;
            SdlManager.Sdl.GetDisplayDPI(displayIndex, &ddpi, null, null);

            return ddpi == 0 ? 1.0f : ddpi / 96.0f;
        }
    }

    public bool IsFocused
    {
        get
        {
            return ((WindowFlags)SdlManager.Sdl.GetWindowFlags(window)).HasFlag(WindowFlags.InputFocus);
        }
    }

    public IVkSurface VkSurface
    {
        get
        {
            return vkSurface ??= new SdlVkSurface(window);
        }
    }

    public void Show()
    {
        if (isLoopRunning)
        {
            return;
        }

        WindowFlags flags = IsVisible ? WindowFlags.Shown : WindowFlags.Hidden;

        switch (State)
        {
            case WindowState.Normal:
                flags |= WindowFlags.Resizable;
                break;
            case WindowState.Minimized:
                flags |= WindowFlags.Minimized;
                break;
            case WindowState.Maximized:
                flags |= WindowFlags.Maximized;
                break;
            case WindowState.Fullscreen:
                flags |= WindowFlags.Fullscreen;
                break;
        }

        switch (Border)
        {
            case WindowBorder.Resizable:
                flags |= WindowFlags.Resizable;
                break;
            case WindowBorder.Fixed:
                flags |= WindowFlags.Borderless;
                break;
            case WindowBorder.Hidden:
                flags |= WindowFlags.Borderless;
                break;
        }

        if (TopMost)
        {
            flags |= WindowFlags.AlwaysOnTop;
        }

        if (!ShowInTaskbar)
        {
            flags |= WindowFlags.SkipTaskbar;
        }

        flags |= WindowFlags.Vulkan;

        window = SdlManager.Sdl.CreateWindow(Encoding.UTF8.GetBytes(Title).AsPointer(),
                                             Position.X,
                                             Position.Y,
                                             Size.X,
                                             Size.Y,
                                             (uint)flags);

        SdlManager.Sdl.ShowWindow(window);

        WindowManager.AddWindow(this);

        isLoopRunning = true;

        Loaded?.Invoke(this, EventArgs.Empty);

        updateStopwatch.Start();
        renderStopwatch.Start();
        lifetimeStopwatch.Start();
    }

    public void Close()
    {
        if (!isLoopRunning)
        {
            return;
        }

        vkSurface?.Dispose();

        isLoopRunning = false;

        SdlManager.Sdl.DestroyWindow(window);

        WindowManager.RemoveWindow(this);

        Unloaded?.Invoke(this, EventArgs.Empty);

        updateStopwatch.Stop();
        renderStopwatch.Stop();
        lifetimeStopwatch.Stop();
    }

    public void DoEvents()
    {
        uint id = SdlManager.Sdl.GetWindowID(window);

        foreach (Event @event in SdlManager.Events)
        {
            if (@event.Window.WindowID != id)
            {
                continue;
            }

            OnEvent(@event);
        }
    }

    public void DoUpdate()
    {
        double delta = updateStopwatch.Elapsed.TotalSeconds;

        if (delta >= updatePeriod)
        {
            Update?.Invoke(this, new TimeEventArgs(delta, lifetimeStopwatch.Elapsed.TotalSeconds));

            updateStopwatch.Restart();
        }
    }

    public void DoRender()
    {
        double delta = renderStopwatch.Elapsed.TotalSeconds;

        if (delta >= renderPeriod)
        {
            Render?.Invoke(this, new TimeEventArgs(delta, lifetimeStopwatch.Elapsed.TotalSeconds));

            renderStopwatch.Restart();
        }
    }

    private void OnEvent(Event @event)
    {
        EventType type = (EventType)@event.Type;

        switch (type)
        {
            case EventType.Windowevent:
                ProcessWindowEvent(@event.Window);
                break;
            case EventType.Keydown:
                ProcessKeyboardEvent(@event.Key, true);
                break;
            case EventType.Keyup:
                ProcessKeyboardEvent(@event.Key, false);
                break;
            case EventType.Textinput:
                ProcessTextInputEvent(@event.Text);
                break;
            case EventType.Mousemotion:
                MouseMove?.Invoke(this, new ValueEventArgs<Vector2D<int>>(new Vector2D<int>(@event.Motion.X, @event.Motion.Y)));
                break;
            case EventType.Mousebuttondown:
                ProcessMouseButtonEvent(@event.Button, true);
                break;
            case EventType.Mousebuttonup:
                ProcessMouseButtonEvent(@event.Button, false);
                break;
            case EventType.Mousewheel:
                MouseWheel?.Invoke(this, new ValueEventArgs<Vector2D<int>>(new Vector2D<int>(@event.Wheel.X, @event.Wheel.Y)));
                break;
        }
    }

    private void ProcessWindowEvent(WindowEvent windowEvent)
    {
        WindowEventID windowEventID = (WindowEventID)windowEvent.Event;

        switch (windowEventID)
        {
            case WindowEventID.Moved:
                PositionChanged?.Invoke(this, new ValueEventArgs<Vector2D<int>>(Position));
                break;
            case WindowEventID.Resized:
                SizeChanged?.Invoke(this, new ValueEventArgs<Vector2D<int>>(Size));
                break;
            case WindowEventID.Minimized:
            case WindowEventID.Maximized:
            case WindowEventID.Restored:
                StateChanged?.Invoke(this, new ValueEventArgs<WindowState>(State));
                break;
            case WindowEventID.Close:
                Close();
                break;
        }
    }

    private void ProcessKeyboardEvent(KeyboardEvent keyboardEvent, bool isKeyDown)
    {
        Key key = SdlManager.GetKey(keyboardEvent.Keysym.Scancode);
        KeyModifiers modifiers = SdlManager.GetKeyModifiers((Keymod)keyboardEvent.Keysym.Mod);

        if (isKeyDown)
        {
            KeyDown?.Invoke(this, new KeyEventArgs(key, modifiers));
        }
        else
        {
            KeyUp?.Invoke(this, new KeyEventArgs(key, modifiers));
        }
    }

    private void ProcessTextInputEvent(TextInputEvent textInputEvent)
    {
        const int charSize = 32;

        var chars = stackalloc char[charSize];
        Encoding.UTF8.GetChars(&textInputEvent.Text[0], charSize, chars, charSize);

        for (int i = 0; i < charSize; i++)
        {
            if (chars[i] == '\0')
            {
                break;
            }

            KeyChar?.Invoke(this, new ValueEventArgs<char>(chars[i]));
        }
    }

    private void ProcessMouseButtonEvent(MouseButtonEvent mouseButtonEvent, bool isMouseDown)
    {
        MouseButton button = SdlManager.GetMouseButton(mouseButtonEvent.Button);

        if (isMouseDown)
        {
            MouseDown?.Invoke(this, new MouseButtonEventArgs(button, new Vector2D<int>(mouseButtonEvent.X, mouseButtonEvent.Y)));
        }
        else
        {
            MouseUp?.Invoke(this, new MouseButtonEventArgs(button, new Vector2D<int>(mouseButtonEvent.X, mouseButtonEvent.Y)));

            if (mouseButtonEvent.Clicks == 1)
            {
                Click?.Invoke(this, new MouseButtonEventArgs(button, new Vector2D<int>(mouseButtonEvent.X, mouseButtonEvent.Y)));
            }
            else if (mouseButtonEvent.Clicks == 2)
            {
                DoubleClick?.Invoke(this, new MouseButtonEventArgs(button, new Vector2D<int>(mouseButtonEvent.X, mouseButtonEvent.Y)));
            }
        }
    }
}
