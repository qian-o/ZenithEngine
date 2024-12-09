using Silk.NET.SDL;
using ZenithEngine.Windowing.Interfaces;

namespace ZenithEngine.Windowing;

public static unsafe class WindowManager
{
    private static readonly List<IWindow> windows = [];
    private static readonly List<IWindow> addWindows = [];
    private static readonly List<IWindow> removeWindows = [];

    public static Sdl Sdl { get; } = Sdl.GetApi();

    public static List<Event> Events { get; } = [];

    public static void PollEvents()
    {
        Events.Clear();

        Event ev;
        while (Sdl.PollEvent(&ev) == (int)SdlBool.True)
        {
            Events.Add(ev);
        }
    }

    public static void Loop()
    {
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
    }

    public static IWindow CreateWindow()
    {
        return new Window();
    }

    internal static void AddLoop(IWindow window)
    {
        addWindows.Add(window);
    }

    internal static void RemoveLoop(IWindow window)
    {
        removeWindows.Add(window);
    }
}
