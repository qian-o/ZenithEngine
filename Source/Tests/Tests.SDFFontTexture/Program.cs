using Graphics.Core;
using Graphics.Vulkan;

internal sealed unsafe class Program
{
    private static GraphicsDevice _device = null!;
    private static CommandList _mainCommandList = null!;

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
            commandList.Begin();
            {
                commandList.SetFramebuffer(device.MainSwapchain.Framebuffer);
                commandList.ClearColorTarget(0, RgbaFloat.Black);
                commandList.ClearDepthStencil(1.0f);

                Render(a, b);

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
        _mainCommandList = commandList;

        window.Run();
    }

    private static void Load(object? sender, LoadEventArgs e)
    {
        Console.WriteLine("Device: " + _device);
        Console.WriteLine("MainCommandList: " + _mainCommandList);
    }

    private static void Update(object? sender, UpdateEventArgs e)
    {
    }

    private static void Render(object? sender, RenderEventArgs e)
    {
    }

    private static void Resize(object? sender, ResizeEventArgs e)
    {
    }
}
