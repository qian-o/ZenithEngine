using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.SDL;
using Silk.NET.Windowing;

namespace Graphics.Core;

public unsafe partial class SdlWindow
{
    private bool _swapchainExpired;

    public event EventHandler<LoadEventArgs>? Load;
    public event EventHandler<UpdateEventArgs>? Update;
    public event EventHandler<RenderEventArgs>? Render;
    public event EventHandler<MoveEventArgs>? Move;
    public event EventHandler<ResizeEventArgs>? Resize;
    public event EventHandler<ClosingEventArgs>? Closing;

    public string Title
    {
        get => _window.Title;
        set => _window.Title = value;
    }

    public Vector2 Position
    {
        get => new(_window.Position.X, _window.Position.Y);
        set => _window.Position = new Vector2D<int>((int)value.X, (int)value.Y);
    }

    public Vector2 Size
    {
        get => new(_window.Size.X, _window.Size.Y);
        set => _window.Size = new Vector2D<int>((int)value.X, (int)value.Y);
    }

    public Vector2 FramebufferSize
    {
        get => new(_window.FramebufferSize.X, _window.FramebufferSize.Y);
    }

    public float DpiScale
    {
        get
        {
            int displayIndex = _sdl.GetWindowDisplayIndex((SDLWindow*)_window.Handle);

            float ddpi;
            _sdl.GetDisplayDPI(displayIndex, &ddpi, null, null);

            return ddpi == 0 ? 1.0f : ddpi / 96.0f;
        }
    }

    public Vector2 MinimumSize
    {
        get
        {
            int w, h;
            _sdl.GetWindowMinimumSize((SDLWindow*)_window.Handle, &w, &h);

            return new Vector2(w, h);
        }
        set
        {
            if (value.X <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "X must be greater than zero.");
            }

            if (value.Y <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Y must be greater than zero.");
            }

            _sdl.SetWindowMinimumSize((SDLWindow*)_window.Handle, (int)value.X, (int)value.Y);
        }
    }

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

    public bool ShowInTaskbar { get; set; } = true;

    public bool IsFocused
    {
        get
        {
            WindowFlags windowFlags = (WindowFlags)_sdl.GetWindowFlags((SDLWindow*)_window.Handle);

            return windowFlags.HasFlag(WindowFlags.InputFocus);
        }
    }

    public float Opacity
    {
        get
        {
            float opacity = 0;
            _sdl.GetWindowOpacity((SDLWindow*)_window.Handle, &opacity);

            return opacity;
        }
        set
        {
            _sdl.SetWindowOpacity((SDLWindow*)_window.Handle, value);
        }
    }

    public void Run()
    {
        _window.IsVisible = true;

        DoLoad();

        _window.Run();
    }

    public void Show()
    {
        _window.IsVisible = true;

        DoLoad();
    }

    public void Focus()
    {
        _sdl.RaiseWindow((SDLWindow*)_window.Handle);
    }

    public void PollEvents()
    {
        _window.DoEvents();
    }

    private void AssemblyStatusEvent()
    {
        _window.Load += DoLoad;

        _window.Update += (d) =>
        {
            Update?.Invoke(this, new UpdateEventArgs((float)d, (float)_window.Time));
        };

        _window.Render += (d) =>
        {
            Render?.Invoke(this, new RenderEventArgs((float)d, (float)_window.Time));
        };

        _window.Move += (v) =>
        {
            Move?.Invoke(this, new MoveEventArgs(v.X, v.Y));
        };

        _window.FramebufferResize += (v) =>
        {
            Resize?.Invoke(this, new ResizeEventArgs((uint)v.X, (uint)v.Y));
        };

        _window.Closing += () =>
        {
            Closing?.Invoke(this, new ClosingEventArgs());
        };

        _window.StateChanged += (s) =>
        {
            if (s == WindowState.Minimized)
            {
                _swapchainExpired = true;
            }
            else
            {
                if (_swapchainExpired)
                {
                    Resize?.Invoke(this, new ResizeEventArgs((uint)_window.FramebufferSize.X, (uint)_window.FramebufferSize.Y));

                    _swapchainExpired = false;
                }
            }
        };
    }

    private void DoLoad()
    {
        Load?.Invoke(this, new LoadEventArgs());
        Resize?.Invoke(this, new ResizeEventArgs((uint)_window.FramebufferSize.X, (uint)_window.FramebufferSize.Y));
    }
}
