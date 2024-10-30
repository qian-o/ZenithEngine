﻿using Graphics.Windowing.Interfaces;
using Graphics.Windowing.Structs;

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
            SdlManager.PollEvents();

            foreach (IWindow window in windows)
            {
                window.DoEvents();
            }

            Parallel.ForEach(windows, window =>
            {
                window.DoUpdate();
            });

            Parallel.ForEach(windows, window =>
            {
                window.DoRender();
            });

            foreach (IWindow window in windowsToAdd)
            {
                windows.Add(window);
            }

            foreach (IWindow window in windowsToRemove)
            {
                windows.Remove(window);
            }

            windowsToAdd.Clear();
            windowsToRemove.Clear();

            if (windows.Count == 0)
            {
                Stop();
            }
        }
    }

    public static void Stop()
    {
        isRunning = false;
    }

    public static bool WindowFocused()
    {
        foreach (IWindow window in windows)
        {
            if (window.IsFocused)
            {
                return true;
            }
        }

        return false;
    }

    public static void SetTextInputRect(int x, int y, int w, int h)
    {
        SdlManager.SetTextInputRect(x, y, w, h);
    }

    public static int GetDisplayCount()
    {
        return SdlManager.GetDisplayCount();
    }

    public static Display GetDisplay(int index)
    {
        return SdlManager.GetDisplay(index);
    }

    public static Display[] GetDisplays()
    {
        int displayCount = GetDisplayCount();

        Display[] displays = new Display[displayCount];

        for (int i = 0; i < displayCount; i++)
        {
            displays[i] = GetDisplay(i);
        }

        return displays;
    }

    internal static void AddWindow(IWindow window)
    {
        windowsToAdd.Add(window);
    }

    internal static void RemoveWindow(IWindow window)
    {
        windowsToRemove.Add(window);
    }
}
