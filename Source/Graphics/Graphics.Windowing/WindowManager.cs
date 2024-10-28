using Graphics.Windowing.Interfaces;

namespace Graphics.Windowing;

public static class WindowManager
{
    private static readonly List<IWindow> windows = [];
    private static readonly List<IWindow> windowsToAdd = [];
    private static readonly List<IWindow> windowsToRemove = [];

    private static bool isRunning;

    public static void Loop()
    {
        isRunning = true;

        while (isRunning)
        {
            foreach (IWindow window in windows)
            {
                // Update window
            }

            foreach (IWindow window in windowsToAdd)
            {
                windows.Add(window);
            }

            foreach (IWindow window in windowsToRemove)
            {
                windows.Remove(window);
            }
        }
    }

    public static void Stop()
    {
        isRunning = false;
    }

    internal static void AddWindow(IWindow window)
    {
        windowsToAdd.Add(window);
    }

    internal static void RemoveWindow(IWindow window)
    {
        windowsToRemove.Remove(window);
    }
}
