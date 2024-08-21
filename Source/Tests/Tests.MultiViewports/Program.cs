using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;

internal sealed unsafe class Program
{
    private static void Main(string[] _)
    {
        using Window window = Window.CreateWindowByVulkan();
        window.Title = "Tests.MultiViewports";
        window.MinimumSize = new(100, 100);

        using Context context = new();
        using GraphicsDevice device = context.CreateGraphicsDevice(context.EnumeratePhysicalDevices().First(), window);
        using CommandList commandList = device.ResourceFactory.CreateGraphicsCommandList();
        using ImGuiController imGuiController = new(window,
                                                    device,
                                                    new ImGuiFontConfig("Assets/Fonts/SIMYOU.TTF", 14, (a) => (nint)a.Fonts.GetGlyphRangesChineseFull()),
                                                    ImGuiSizeConfig.Default);

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
                commandList.SetFramebuffer(device.MainSwapchain.Framebuffer);
                commandList.ClearColorTarget(0, RgbaFloat.Black);
                commandList.ClearDepthStencil(1.0f);

                imGuiController.Render(commandList);
            }
            commandList.End();

            device.SubmitCommandsAndSwapBuffers(commandList, device.MainSwapchain);

            imGuiController.PlatformSwapBuffers();
        }

        void Window_Resize(object? sender, ResizeEventArgs e)
        {
            device.MainSwapchain.Resize(e.Width, e.Height);
        }
    }
}