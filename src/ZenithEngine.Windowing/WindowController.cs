using Silk.NET.SDL;
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
        while (WindowUtils.Sdl.PollEvent(&@event) == (int)SdlBool.True)
        {
            Events.Add(@event);
        }
    }

    public static void Loop()
    {
        IsLooping = true;

        while (windows.Count > 0)
        {
            PollEvents();

            foreach (IWindow window in windows)
            {
                window.DoEvents();
            }

            foreach (IWindow window in addWindows)
            {
                windows.Add(window);
            }

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

    public static IWindow CreateWindow(string title = "Window",
                                       uint width = 800,
                                       uint height = 600)
    {
        Window window = new()
        {
            Title = title,
            Size = new(width, height)
        };

        return window;
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
