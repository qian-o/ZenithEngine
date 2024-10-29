using System.Text;
using Graphics.Core.Helpers;
using Graphics.Windowing.Enums;
using Graphics.Windowing.Interactivity;
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

    public event EventHandler<PropertyEventArgs<WindowState>>? StateChanged;

    public event EventHandler<PropertyEventArgs<Vector2D<int>>>? PositionChanged;

    public event EventHandler<PropertyEventArgs<Vector2D<int>>>? SizeChanged;

    public event EventHandler<KeyEventArgs>? KeyDown;

    public event EventHandler<KeyEventArgs>? KeyUp;

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
                SdlManager.Sdl.SetWindowMinimumSize(window, (int)value.X, (int)value.Y);
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
                SdlManager.Sdl.SetWindowMaximumSize(window, (int)value.X, (int)value.Y);
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
                SdlManager.Sdl.SetWindowPosition(window, (int)value.X, (int)value.Y);
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
                SdlManager.Sdl.SetWindowSize(window, (int)value.X, (int)value.Y);
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
    }

    public void Close()
    {
        if (!isLoopRunning)
        {
            return;
        }

        SdlManager.Sdl.DestroyWindow(window);

        WindowManager.RemoveWindow(this);

        isLoopRunning = false;

        Unloaded?.Invoke(this, EventArgs.Empty);
    }

    public void HandleEvents()
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
                break;
            case EventType.Mousemotion:
                break;
            case EventType.Mousebuttondown:
                break;
            case EventType.Mousebuttonup:
                break;
            case EventType.Mousewheel:
                break;
        }
    }

    private void ProcessWindowEvent(WindowEvent windowEvent)
    {
        WindowEventID windowEventID = (WindowEventID)windowEvent.Event;

        switch (windowEventID)
        {
            case WindowEventID.Moved:
                PositionChanged?.Invoke(this, new PropertyEventArgs<Vector2D<int>>(Position));
                break;
            case WindowEventID.Resized:
                SizeChanged?.Invoke(this, new PropertyEventArgs<Vector2D<int>>(Size));
                break;
            case WindowEventID.Minimized:
            case WindowEventID.Maximized:
            case WindowEventID.Restored:
                StateChanged?.Invoke(this, new PropertyEventArgs<WindowState>(State));
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
}
