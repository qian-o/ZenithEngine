﻿using Graphics.Core;
using Renderer.Components;

namespace Renderer;

internal sealed unsafe class Program
{
    private static MainScene? _mainScene;

    private static void Main(string[] _)
    {
        using Window window = Window.CreateWindowByVulkan();
        window.Load += Window_Load;
        window.Close += Window_Close;

        window.Run();
    }

    private static void Window_Load(object? sender, LoadEventArgs e)
    {
        _mainScene = new MainScene((Window)sender!);
    }

    private static void Window_Close(object? sender, CloseEventArgs e)
    {
        _mainScene?.Dispose();
    }
}