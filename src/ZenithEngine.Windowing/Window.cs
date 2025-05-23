﻿using System.Diagnostics;
using System.Text;
using Silk.NET.SDL;
using ZenithEngine.Common.Enums;
using ZenithEngine.Windowing.Interfaces;

namespace ZenithEngine.Windowing;

internal unsafe partial class Window : IWindow
{
    private readonly Stopwatch updateStopwatch = new();
    private readonly Stopwatch renderStopwatch = new();
    private readonly Stopwatch lifetimeStopwatch = new();

    public SdlWindow* Handle;

    public void Show()
    {
        if (!TryInitialize())
        {
            return;
        }

        updateStopwatch.Start();
        renderStopwatch.Start();
        lifetimeStopwatch.Start();

        Loaded?.Invoke(this, EventArgs.Empty);
    }

    public void Close()
    {
        if (!TryUninitialize())
        {
            return;
        }

        updateStopwatch.Stop();
        renderStopwatch.Stop();
        lifetimeStopwatch.Stop();

        Unloaded?.Invoke(this, EventArgs.Empty);

        updateStopwatch.Reset();
        renderStopwatch.Reset();
        lifetimeStopwatch.Reset();
    }

    public void Center()
    {
        if (!IsInitialized())
        {
            return;
        }

        Display display = WindowUtils.GetDisplays()[WindowUtils.Sdl.GetWindowDisplayIndex(Handle)];

        int x = display.MainPosition.X + (int)((display.MainSize.X / 2) - (Size.X / 2));
        int y = display.MainPosition.Y + (int)((display.MainSize.Y / 2) - (Size.Y / 2));

        WindowUtils.Sdl.SetWindowPosition(Handle, x, y);
    }

    public void Focus()
    {
        if (!IsInitialized())
        {
            return;
        }

        WindowUtils.Sdl.RaiseWindow(Handle);
    }

    public void DoEvents()
    {
        if (!IsInitialized())
        {
            return;
        }

        uint id = WindowUtils.Sdl.GetWindowID(Handle);

        foreach (Event @event in WindowController.Events)
        {
            if (@event.Window.WindowID != id)
            {
                continue;
            }

            ProcessEvent(@event);
        }
    }

    public void DoUpdate()
    {
        double delta = updateStopwatch.Elapsed.TotalSeconds;

        if (delta >= updatePeriod)
        {
            updateStopwatch.Restart();

            Update?.Invoke(this, new(delta, lifetimeStopwatch.Elapsed.TotalSeconds));
        }
    }

    public void DoRender()
    {
        double delta = renderStopwatch.Elapsed.TotalSeconds;

        if (delta >= renderPeriod)
        {
            renderStopwatch.Restart();

            Render?.Invoke(this, new(delta, lifetimeStopwatch.Elapsed.TotalSeconds));
        }
    }

    private bool IsInitialized()
    {
        return Handle is not null;
    }

    private bool TryInitialize()
    {
        if (IsInitialized())
        {
            return false;
        }

        WindowFlags flags = WindowFlags.Shown;

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
                flags &= ~WindowFlags.Resizable;
                break;
            case WindowBorder.Hidden:
                flags |= WindowFlags.Borderless;
                break;
        }

        if (TopMost)
        {
            flags |= WindowFlags.AlwaysOnTop;
        }

        Handle = WindowUtils.Sdl.CreateWindow(Title,
                                              Position.X,
                                              Position.Y,
                                              (int)Size.X,
                                              (int)Size.Y,
                                              (uint)flags);

        WindowUtils.Sdl.SetWindowMinimumSize(Handle, (int)MinimumSize.X, (int)MinimumSize.Y);
        WindowUtils.Sdl.SetWindowMaximumSize(Handle, (int)MaximumSize.X, (int)MaximumSize.Y);
        WindowUtils.Sdl.SetWindowOpacity(Handle, Opacity);

        WindowController.AddLoop(this);

        return true;
    }

    private bool TryUninitialize()
    {
        if (!IsInitialized())
        {
            return false;
        }

        WindowController.RemoveLoop(this);

        WindowUtils.Sdl.DestroyWindow(Handle);

        Handle = null;

        return true;
    }

    private void ProcessEvent(Event @event)
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
                MouseMove?.Invoke(this, new(new(@event.Motion.X, @event.Motion.Y)));
                break;
            case EventType.Mousebuttondown:
                ProcessMouseButtonEvent(@event.Button, true);
                break;
            case EventType.Mousebuttonup:
                ProcessMouseButtonEvent(@event.Button, false);
                break;
            case EventType.Mousewheel:
                MouseWheel?.Invoke(this, new(new(@event.Wheel.X, @event.Wheel.Y)));
                break;
        }
    }

    private void ProcessWindowEvent(WindowEvent windowEvent)
    {
        WindowEventID windowEventID = (WindowEventID)windowEvent.Event;

        switch (windowEventID)
        {
            case WindowEventID.Moved:
                PositionChanged?.Invoke(this, new(Position));
                break;
            case WindowEventID.Resized:
                SizeChanged?.Invoke(this, new(Size));
                break;
            case WindowEventID.Minimized or WindowEventID.Maximized:
                StateChanged?.Invoke(this, new(State));
                break;
            case WindowEventID.Restored:
                StateChanged?.Invoke(this, new(State));
                SizeChanged?.Invoke(this, new(Size));
                break;
            case WindowEventID.Close:
                Close();
                break;
        }
    }

    private void ProcessKeyboardEvent(KeyboardEvent keyboardEvent, bool isKeyDown)
    {
        Key key = WindowUtils.GetKey(keyboardEvent.Keysym.Scancode);
        KeyModifiers modifiers = WindowUtils.GetKeyModifiers((Keymod)keyboardEvent.Keysym.Mod);

        if (isKeyDown)
        {
            KeyDown?.Invoke(this, new(key, modifiers));
        }
        else
        {
            KeyUp?.Invoke(this, new(key, modifiers));
        }
    }

    private void ProcessTextInputEvent(TextInputEvent textInputEvent)
    {
        const int charSize = 32;

        char* chars = stackalloc char[charSize];
        Encoding.UTF8.GetChars(&textInputEvent.Text[0], charSize, chars, charSize);

        for (int i = 0; i < charSize; i++)
        {
            if (chars[i] is '\0')
            {
                break;
            }

            KeyChar?.Invoke(this, new(chars[i]));
        }
    }

    private void ProcessMouseButtonEvent(MouseButtonEvent mouseButtonEvent, bool isMouseDown)
    {
        MouseButton button = WindowUtils.GetMouseButton(mouseButtonEvent.Button);

        if (isMouseDown)
        {
            MouseDown?.Invoke(this, new(button,
                                        new(mouseButtonEvent.X, mouseButtonEvent.Y),
                                        1));
        }
        else
        {
            MouseUp?.Invoke(this, new(button,
                                      new(mouseButtonEvent.X, mouseButtonEvent.Y),
                                      1));

            if (mouseButtonEvent.Clicks >= 1)
            {
                Click?.Invoke(this, new(button,
                                        new(mouseButtonEvent.X, mouseButtonEvent.Y),
                                        mouseButtonEvent.Clicks));
            }
        }
    }
}
