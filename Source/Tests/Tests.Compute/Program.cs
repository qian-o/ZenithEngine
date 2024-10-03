﻿using Graphics.Core;
using Graphics.Vulkan;
using Tests.Core;

namespace Tests.Compute;

internal sealed unsafe class Program
{
    private static GraphicsDevice _device = null!;
    private static ImGuiController _imGuiController = null!;
    private static View[] _views = null!;

    private static void Main(string[] _)
    {
        using SdlWindow window = SdlWindow.CreateWindowByVulkan();
        window.Title = "Tests.Compute";
        window.MinimumSize = new(100, 100);

        using Context context = new();
        using GraphicsDevice device = context.CreateGraphicsDevice(context.GetBestPhysicalDevice(), window);
        using ImGuiController imGuiController = new(window,
                                                    device,
                                                    new ImGuiFontConfig("Assets/Fonts/msyh.ttf", 16, (a) => (nint)a.Fonts.GetGlyphRangesChineseFull()),
                                                    ImGuiSizeConfig.Default);
        using CommandList commandList = device.Factory.CreateGraphicsCommandList();

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

        window.Closing += Closing;

        _device = device;
        _imGuiController = imGuiController;

        window.Run();
    }

    private static void Load(object? sender, LoadEventArgs e)
    {
        _views = [new MainView(_device, _imGuiController)];
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

    private static void Closing(object? sender, ClosingEventArgs e)
    {
        foreach (View view in _views)
        {
            view.Dispose();
        }
    }
}
