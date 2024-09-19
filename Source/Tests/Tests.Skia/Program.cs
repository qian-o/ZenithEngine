﻿using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using SkiaSharp;
using Tests.Core;

namespace Tests.Skia;

internal sealed unsafe class Program
{
    private static GraphicsDevice _device = null!;
    private static ImGuiController _imGuiController = null!;
    private static GRContext _grContext = null!;
    private static View[] _views = null!;

    private static void Main(string[] _)
    {
        using Window window = Window.CreateWindowByVulkan();
        window.Title = "Tests.Skia";
        window.MinimumSize = new(100, 100);

        using Context context = new();
        using GraphicsDevice device = context.CreateGraphicsDevice(context.GetBestPhysicalDevice(), window);
        using ImGuiController imGuiController = new(window,
                                                    device,
                                                    new ImGuiFontConfig("Assets/Fonts/msyh.ttf", 16, (a) => (nint)a.Fonts.GetGlyphRangesChineseFull()),
                                                    ImGuiSizeConfig.Default);
        using GRContext grContext = SkiaVk.CreateContext(device);

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

            _grContext.Flush(true);
            _grContext.PurgeUnusedResources(1000);

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
        _grContext = grContext;

        window.Run();
    }

    private static void Load(object? sender, LoadEventArgs e)
    {
        _views =
        [
            new AnimationView(Path.Combine("Assets", "LottieFiles", "demo1.json"), _device, _imGuiController, _grContext),
            new AnimationView(Path.Combine("Assets", "LottieFiles", "demo2.json"), _device, _imGuiController, _grContext)
        ];
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

        ImGui.Begin("Tests.Skia");
        {
            ImGui.Text($"FPS: {1.0f / e.DeltaTime}");

            ImGui.Separator();

            ImGui.Text($"Total Time: {e.TotalTime}");

            ImGui.Separator();

            ImGui.Text($"Delta Time: {e.DeltaTime}");

            ImGui.End();
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
