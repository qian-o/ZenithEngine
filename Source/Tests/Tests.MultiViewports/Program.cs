using Graphics.Core;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.ImGui;
using Graphics.Windowing;
using Graphics.Windowing.Events;
using Hexa.NET.ImGui;
using Silk.NET.Maths;

internal sealed unsafe class Program
{
    private static void Main(string[] _)
    {
        SdlWindow window = new()
        {
            Title = "Tests.MultiViewports",
            MinimumSize = new(100, 100)
        };

        window.Show();

        using Context context = new();
        using GraphicsDevice device = context.CreateGraphicsDevice(context.GetBestPhysicalDevice());
        using Swapchain swapchain = device.Factory.CreateSwapchain(new SwapchainDescription(window.VkSurface!, device.GetBestDepthFormat()));
        using ImGuiController imGuiController = new(window,
                                                    () => new SdlWindow(),
                                                    device,
                                                    swapchain.OutputDescription,
                                                    new ImGuiFontConfig("Assets/Fonts/msyh.ttf", 16, (a) => (nint)a.Fonts.GetGlyphRangesChineseFull()),
                                                    ImGuiSizeConfig.Default);
        using CommandList commandList = device.Factory.CreateGraphicsCommandList();

        window.PositionChanged += Window_PositionChanged;
        window.Update += Window_Update;
        window.Render += Window_Render;
        window.Unloaded += Window_Unloaded;

        WindowManager.Loop();

        void Window_PositionChanged(object? sender, ValueEventArgs<Vector2D<int>> e)
        {
            swapchain.Resize();
        }

        void Window_Update(object? sender, TimeEventArgs e)
        {
            imGuiController.Update((float)e.DeltaTime);
        }

        void Window_Render(object? sender, TimeEventArgs e)
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
        
        void Window_Unloaded(object? sender, EventArgs e)
        {
            WindowManager.Stop();
        }
    }
}