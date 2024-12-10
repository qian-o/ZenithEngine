using Silk.NET.Maths;
using Silk.NET.SDL;
using ZenithEngine.Common.Interfaces;
using ZenithEngine.Windowing.Enums;
using ZenithEngine.Windowing.Interfaces;

namespace ZenithEngine.Windowing;

internal unsafe partial class Window : IWindowProperties
{
    private ISurface? surface;
    private string title = "Window";
    private WindowState state = WindowState.Normal;
    private WindowBorder border = WindowBorder.Resizable;
    private bool topMost;
    private bool showInTaskbar = true;
    private Vector2D<int> position = new(100, 100);
    private Vector2D<int> size = new(800, 600);
    private Vector2D<int> minimumSize = new(100, 100);
    private Vector2D<int> maximumSize = Vector2D<int>.Zero;
    private float opacity = 1;
    private double updatePeriod;
    private double renderPeriod;

    public ISurface Surface
    {
        get
        {
            if (!IsInitialized())
            {
                throw new InvalidOperationException("The window is not initialized.");
            }

            return surface ??= new Surface(Handle);
        }
    }

    public string Title
    {
        get => title;
        set
        {
            title = value;

            if (!IsInitialized())
            {
                return;
            }

            WindowUtils.Sdl.SetWindowTitle(Handle, value);
        }
    }

    public WindowState State
    {
        get => state;
        set
        {
            state = value;

            if (!IsInitialized())
            {
                return;
            }

            switch (value)
            {
                case WindowState.Normal:
                    WindowUtils.Sdl.RestoreWindow(Handle);
                    break;
                case WindowState.Minimized:
                    WindowUtils.Sdl.MinimizeWindow(Handle);
                    break;
                case WindowState.Maximized:
                    WindowUtils.Sdl.MaximizeWindow(Handle);
                    break;
                case WindowState.Fullscreen:
                    WindowUtils.Sdl.SetWindowFullscreen(Handle, (uint)WindowFlags.Fullscreen);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    public WindowBorder Border
    {
        get => border;
        set
        {
            border = value;

            if (!IsInitialized())
            {
                return;
            }

            switch (value)
            {
                case WindowBorder.Resizable:
                    WindowUtils.Sdl.SetWindowBordered(Handle, SdlBool.True);
                    WindowUtils.Sdl.SetWindowResizable(Handle, SdlBool.True);
                    break;
                case WindowBorder.Fixed:
                    WindowUtils.Sdl.SetWindowBordered(Handle, SdlBool.True);
                    WindowUtils.Sdl.SetWindowResizable(Handle, SdlBool.False);
                    break;
                case WindowBorder.Hidden:
                    WindowUtils.Sdl.SetWindowBordered(Handle, SdlBool.False);
                    WindowUtils.Sdl.SetWindowResizable(Handle, SdlBool.False);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    public bool TopMost
    {
        get => topMost;
        set
        {
            topMost = value;

            if (!IsInitialized())
            {
                return;
            }

            WindowUtils.Sdl.SetWindowAlwaysOnTop(Handle, value ? SdlBool.True : SdlBool.False);
        }
    }

    public bool ShowInTaskbar
    {
        get => showInTaskbar;
        set
        {
            showInTaskbar = value;

            if (!IsInitialized())
            {
                return;
            }

            // SDL does not support this feature.
            // WindowManager.Sdl.SetWindowSkipTaskbar(Handle, value ? SdlBool.False : SdlBool.True);
        }
    }

    public Vector2D<int> Position
    {
        get => position;
        set
        {
            position = value;

            if (!IsInitialized())
            {
                return;
            }

            WindowUtils.Sdl.SetWindowPosition(Handle, value.X, value.Y);
        }
    }

    public Vector2D<int> Size
    {
        get => size;
        set
        {
            size = value;

            if (!IsInitialized())
            {
                return;
            }

            WindowUtils.Sdl.SetWindowSize(Handle, value.X, value.Y);
        }
    }

    public Vector2D<int> MinimumSize
    {
        get => minimumSize;
        set
        {
            minimumSize = value;

            if (!IsInitialized())
            {
                return;
            }

            WindowUtils.Sdl.SetWindowMinimumSize(Handle, value.X, value.Y);
        }
    }

    public Vector2D<int> MaximumSize
    {
        get => maximumSize;
        set
        {
            maximumSize = value;

            if (!IsInitialized())
            {
                return;
            }

            WindowUtils.Sdl.SetWindowMaximumSize(Handle, value.X, value.Y);
        }
    }

    public float Opacity
    {
        get => opacity;
        set
        {
            opacity = value;

            if (!IsInitialized())
            {
                return;
            }

            WindowUtils.Sdl.SetWindowOpacity(Handle, value);
        }
    }

    public double UpdatePerSecond
    {
        get => updatePeriod <= double.Epsilon ? 0 : 1 / updatePeriod;
        set => updatePeriod = value <= double.Epsilon ? 0 : 1 / value;
    }

    public double RenderPerSecond
    {
        get => renderPeriod <= double.Epsilon ? 0 : 1 / renderPeriod;
        set => renderPeriod = value <= double.Epsilon ? 0 : 1 / value;
    }
}
