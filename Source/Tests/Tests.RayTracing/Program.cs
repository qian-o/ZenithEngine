using Graphics.Core;
using Graphics.Core.Window;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.ImGui;
using Hexa.NET.ImGui;
using Tests.Core;

namespace Tests.RayTracing;

internal sealed unsafe class Program
{
    private static GraphicsDevice _device = null!;
    private static ImGuiController _imGuiController = null!;
    private static View[] _views = null!;

    private static void Main(string[] _)
    {
        using SdlWindow window = SdlWindow.CreateWindowByVulkan();
        window.Title = "Tests.RayTracing";
        window.MinimumSize = new(100, 100);

        using Context context = new();
        using GraphicsDevice device = context.CreateGraphicsDevice(context.GetBestPhysicalDevice());
        using Swapchain swapchain = device.Factory.CreateSwapchain(new SwapchainDescription(window.VkSurface!, device.GetBestDepthFormat()));
        using ImGuiController imGuiController = new(window,
                                                    device,
                                                    swapchain.OutputDescription,
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
                commandList.SetFramebuffer(swapchain.Framebuffer);
                commandList.ClearColorTarget(0, RgbaFloat.Black);
                commandList.ClearDepthStencil(1.0f);

                imGuiController.Render(commandList);
            }
            commandList.End();

            device.SubmitCommandsAndSwapBuffers(commandList, swapchain);

            imGuiController.PlatformSwapBuffers();
        };

        window.Resize += (a, b) =>
        {
            swapchain.Resize();

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

        ImGui.Begin("Tests.RayTracing");
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
