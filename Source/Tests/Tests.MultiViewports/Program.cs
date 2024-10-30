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
    private static readonly SdlWindow mainWindow = new()
    {
        Title = "Tests.MultiViewports",
        MinimumSize = new(100, 100)
    };

    private static Context context = null!;
    private static GraphicsDevice device = null!;
    private static Swapchain swapchain = null!;
    private static ImGuiController imGuiController = null!;
    private static CommandList commandList = null!;

    private static void Main(string[] _)
    {
        mainWindow.Loaded += MainWindow_Loaded;
        mainWindow.Unloaded += MainWindow_Unloaded;
        mainWindow.PositionChanged += MainWindow_PositionChanged;
        mainWindow.Update += MainWindow_Update;
        mainWindow.Render += MainWindow_Render;

        mainWindow.Show();

        WindowManager.Loop();
    }

    private static void MainWindow_Loaded(object? sender, EventArgs e)
    {
        context = new();
        device = context.CreateGraphicsDevice(context.GetBestPhysicalDevice());
        swapchain = device.Factory.CreateSwapchain(new SwapchainDescription(mainWindow.VkSurface!, device.GetBestDepthFormat()));
        imGuiController = new(mainWindow,
                              () => new SdlWindow(),
                              device,
                              swapchain.OutputDescription,
                              new ImGuiFontConfig("Assets/Fonts/msyh.ttf", 16, (a) => (nint)a.Fonts.GetGlyphRangesChineseFull()),
                              ImGuiSizeConfig.Default);
        commandList = device.Factory.CreateGraphicsCommandList();
    }

    private static void MainWindow_Unloaded(object? sender, EventArgs e)
    {
        commandList.Dispose();
        imGuiController.Dispose();
        swapchain.Dispose();
        device.Dispose();
        context.Dispose();

        WindowManager.Stop();
    }

    private static void MainWindow_PositionChanged(object? sender, ValueEventArgs<Vector2D<int>> e)
    {
        swapchain.Resize();
    }

    private static void MainWindow_Update(object? sender, TimeEventArgs e)
    {
        imGuiController.Update((float)e.DeltaTime);
    }

    private static void MainWindow_Render(object? sender, TimeEventArgs e)
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
}