﻿using Graphics.Core;
using Graphics.Vulkan;
using Tests.Core;

namespace Tests.SDFFontTexture;

internal sealed unsafe class Program
{
    private static GraphicsDevice _device = null!;
    private static View[] _views = null!;

    private static void Main(string[] _)
    {
        using Window window = Window.CreateWindowByVulkan();
        window.Title = "Tests.SDFFontTexture";
        window.MinimumSize = new(100, 100);

        using Context context = new();
        using GraphicsDevice device = context.CreateGraphicsDevice(context.EnumeratePhysicalDevices().First(), window);
        using CommandList commandList = device.ResourceFactory.CreateGraphicsCommandList();
        using ImGuiController imGuiController = new(window,
                                                    device,
                                                    new ImGuiFontConfig("Assets/Fonts/MSYH.TTC", 14, (a) => (nint)a.Fonts.GetGlyphRangesChineseFull()));

        window.Load += Load;

        window.Update += (a, b) =>
        {
            imGuiController.Update(b.DeltaTime);

            Update(a, b);
        };

        window.Render += (a, b) =>
        {
            Render(a, b);

            commandList.Begin();
            {
                commandList.SetFramebuffer(device.MainSwapchain.Framebuffer);
                commandList.ClearColorTarget(0, RgbaFloat.Black);
                commandList.ClearDepthStencil(1.0f);

                imGuiController.Render(commandList);
            }
            commandList.End();

            device.SubmitCommandsAndSwapBuffers(commandList, device.MainSwapchain);

            imGuiController.PlatformSwapBuffers();
        };

        window.Resize += (a, b) =>
        {
            device.MainSwapchain.Resize(b.Width, b.Height);

            Resize(a, b);
        };

        _device = device;

        window.Run();
    }

    private static void Load(object? sender, LoadEventArgs e)
    {
        _views = [new MainView(_device)];
    }

    private static void Update(object? sender, UpdateEventArgs e)
    {
        foreach (View view in _views)
        {
            view.Update(e);
        }
    }

    private static void Render(object? sender, RenderEventArgs e)
    {
        foreach (View view in _views)
        {
            view.Render(e);
        }
    }

    private static void Resize(object? sender, ResizeEventArgs e)
    {
    }
}
