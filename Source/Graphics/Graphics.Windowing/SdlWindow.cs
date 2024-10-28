using System.Text;
using Graphics.Core.Helpers;
using Graphics.Windowing.Enums;
using Graphics.Windowing.Interfaces;
using Graphics.Windowing.Structs;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace Graphics.Windowing;

public unsafe class SdlWindow : IWindow
{
    public static readonly Sdl Sdl = Sdl.GetApi();

    private string title = "SdlWindow";
    private WindowState windowState;
    private WindowBorder windowBorder;
    private Vector2D<int> minimumSize;
    private Vector2D<int> maximumSize;
    private Vector2D<int> position;
    private Vector2D<int> size;
    private bool isVisible;
    private bool topMost;
    private bool showInTaskbar;
    private float opacity;

    private Window* window;
    private SdlVkSurface? vkSurface;

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
                Sdl.SetWindowTitle(window, Encoding.UTF8.GetBytes(value).AsPointer());
            }
        }
    }

    public WindowState WindowState
    {
        get
        {
            return windowState;
        }
        set
        {
            windowState = value;

            if (IsCreated)
            {
                switch (value)
                {
                    case WindowState.Normal:
                        Sdl.RestoreWindow(window);
                        break;
                    case WindowState.Minimized:
                        Sdl.MinimizeWindow(window);
                        break;
                    case WindowState.Maximized:
                        Sdl.MaximizeWindow(window);
                        break;
                    case WindowState.Fullscreen:
                        Sdl.SetWindowFullscreen(window, (uint)WindowFlags.Fullscreen);
                        break;
                }
            }
        }
    }

    public WindowBorder WindowBorder
    {
        get
        {
            return windowBorder;
        }
        set
        {
            windowBorder = value;

            if (IsCreated)
            {
                switch (value)
                {
                    case WindowBorder.Resizable:
                        Sdl.SetWindowBordered(window, SdlBool.True);
                        Sdl.SetWindowResizable(window, SdlBool.True);
                        break;
                    case WindowBorder.Fixed:
                        Sdl.SetWindowBordered(window, SdlBool.True);
                        Sdl.SetWindowResizable(window, SdlBool.False);
                        break;
                    case WindowBorder.Hidden:
                        Sdl.SetWindowBordered(window, SdlBool.False);
                        Sdl.SetWindowBordered(window, SdlBool.False);
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
                Sdl.SetWindowMinimumSize(window, (int)value.X, (int)value.Y);
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
                Sdl.SetWindowMaximumSize(window, (int)value.X, (int)value.Y);
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
                Sdl.GetWindowPosition(window, &x, &y);

                return position = new Vector2D<int>(x, y);
            }

            return position;
        }
        set
        {
            position = value;

            if (IsCreated)
            {
                Sdl.SetWindowPosition(window, (int)value.X, (int)value.Y);
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
                Sdl.GetWindowSize(window, &width, &height);

                return size = new Vector2D<int>(width, height);
            }

            return size;
        }
        set
        {
            size = value;

            if (IsCreated)
            {
                Sdl.SetWindowSize(window, (int)value.X, (int)value.Y);
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
                    Sdl.ShowWindow(window);
                }
                else
                {
                    Sdl.HideWindow(window);
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
                    Sdl.SetWindowAlwaysOnTop(window, SdlBool.True);
                }
                else
                {
                    Sdl.SetWindowAlwaysOnTop(window, SdlBool.False);
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
                Sdl.SetWindowOpacity(window, value);
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
            int displayIndex = Sdl.GetWindowDisplayIndex(window);

            float ddpi;
            Sdl.GetDisplayDPI(displayIndex, &ddpi, null, null);

            return ddpi == 0 ? 1.0f : ddpi / 96.0f;
        }
    }

    public bool IsFocused
    {
        get
        {
            return ((WindowFlags)Sdl.GetWindowFlags(window)).HasFlag(WindowFlags.InputFocus);
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
        Init();

        Sdl.ShowWindow(window);

        WindowManager.AddWindow(this);
    }

    public void ShowDialog()
    {
        Init();

        Sdl.ShowWindow(window);

        while (true)
        {
            // Poll events
        }
    }

    public void Close()
    {
        Sdl.DestroyWindow(window);

        WindowManager.RemoveWindow(this);
    }

    private void Init()
    {
        if (window != null)
        {
            return;
        }

        WindowFlags flags = IsVisible ? WindowFlags.Shown : WindowFlags.Hidden;

        switch (WindowState)
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

        switch (WindowBorder)
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

        if (ShowInTaskbar)
        {
            flags |= WindowFlags.SkipTaskbar;
        }

        window = Sdl.CreateWindow(Encoding.UTF8.GetBytes(Title).AsPointer(),
                                  (int)Position.X,
                                  (int)Position.Y,
                                  (int)Size.X,
                                  (int)Size.Y,
                                  (uint)flags);
    }
}
