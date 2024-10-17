using Graphics.Core;
using Graphics.Core.Window;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.ImGui;
using Hexa.NET.ImGui;

internal sealed unsafe class Program
{
    private static void Main(string[] _)
    {
        using SdlWindow window = SdlWindow.CreateWindowByVulkan();
        window.Title = "Tests.MultiViewports";
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

        window.Update += Window_Update;
        window.Render += Window_Render;
        window.Resize += Window_Resize;

        window.Run();

        void Window_Update(object? sender, UpdateEventArgs e)
        {
            imGuiController.Update(e.DeltaTime);
        }

        void Window_Render(object? sender, RenderEventArgs e)
        {
            ImGui.ShowDemoWindow();

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
        }

        void Window_Resize(object? sender, ResizeEventArgs e)
        {
            swapchain.Resize();
        }
    }
}