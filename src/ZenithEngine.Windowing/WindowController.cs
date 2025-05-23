﻿using Silk.NET.SDL;
using ZenithEngine.Windowing.Interfaces;

namespace ZenithEngine.Windowing;

public static unsafe class WindowController
{
    private static readonly List<IWindow> windows = [];
    private static readonly List<IWindow> addWindows = [];
    private static readonly List<IWindow> removeWindows = [];

    public static bool IsLooping { get; private set; }

    public static List<Event> Events { get; } = [];

    public static void PollEvents()
    {
        Events.Clear();

        Event @event;
        while ((SdlBool)WindowUtils.Sdl.PollEvent(&@event) is SdlBool.True)
        {
            Events.Add(@event);
        }
    }

    public static void Loop(bool autoExit)
    {
        IsLooping = true;

        Func<bool> exit = autoExit ? (static () => windows.Count is 0) : (static () => !IsLooping);

        while (!exit())
        {
            PollEvents();

            foreach (IWindow window in windows)
            {
                window.DoEvents();
            }

            windows.AddRange(addWindows);

            foreach (IWindow window in removeWindows)
            {
                windows.Remove(window);
            }

            addWindows.Clear();
            removeWindows.Clear();

            foreach (IWindow window in windows)
            {
                window.DoUpdate();
            }

            foreach (IWindow window in windows)
            {
                window.DoRender();
            }
        }

        IsLooping = false;
    }

    public static void Exit()
    {
        IsLooping = false;
    }

    public static IWindow CreateWindow(string title = "Window",
                                       uint width = 800,
                                       uint height = 600)
    {
        return (Window)new()
        {
            Title = title,
            Size = new(width, height)
        };
    }

    internal static void AddLoop(IWindow window)
    {
        if (IsLooping)
        {
            addWindows.Add(window);
        }
        else
        {
            windows.Add(window);
        }
    }

    internal static void RemoveLoop(IWindow window)
    {
        if (IsLooping)
        {
            removeWindows.Add(window);
        }
        else
        {
            windows.Remove(window);
        }
    }
}
