using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.Editor.Models;
using ZenithEngine.Editor.Views;
using ZenithEngine.ImGuiWrapper;
using ZenithEngine.Windowing;
using ZenithEngine.Windowing.Interfaces;

namespace ZenithEngine.Editor;

internal static class App
{
    static App()
    {
        System = new();

        Context = GraphicsContext.Create(System.Backend);
        Context.CreateDevice();

        MainView = new MainView();

        MainWindow = WindowController.CreateWindow("ZenithEngine Editor", 1280, 720);
        MainWindow.State = WindowState.Maximized;
        MainWindow.Render += (_, _) => MainView.Render();
    }

    public static SystemConfig System { get; }

    public static GraphicsContext Context { get; }

    public static MainView MainView { get; }

    public static IWindow MainWindow { get; }

    public static SwapChain SwapChain { get; private set; } = null!;

    public static ImGuiController ImGuiController { get; private set; } = null!;

    public static CommandProcessor GraphicsQueue { get; private set; } = null!;

    public static CommandProcessor ComputeQueue { get; private set; } = null!;

    public static CommandProcessor CopyQueue { get; private set; } = null!;

    public static void Run()
    {
        MainWindow.Loaded += (_, _) =>
        {
            SwapChainDesc swapChainDesc = new() { Surface = MainWindow.Surface };

            SwapChain = Context.Factory.CreateSwapChain(in swapChainDesc);

            ImGuiController = new(MainWindow, Context, SwapChain.FrameBuffer.Output);

            GraphicsQueue = Context.Factory.CreateCommandProcessor(CommandProcessorType.Graphics);

            ComputeQueue = Context.Factory.CreateCommandProcessor(CommandProcessorType.Compute);

            CopyQueue = Context.Factory.CreateCommandProcessor(CommandProcessorType.Copy);
        };

        MainWindow.Update += (_, args) => ImGuiController.Update(args.DeltaTime, MainWindow.Size);

        MainWindow.Render += (_, _) =>
        {
            CommandBuffer commandBuffer = GraphicsQueue.CommandBuffer();

            commandBuffer.Begin();

            ImGuiController.PrepareResources(commandBuffer);

            commandBuffer.BeginRendering(SwapChain.FrameBuffer, new(1, options: ClearOptions.All));

            ImGuiController.Render(commandBuffer);

            commandBuffer.EndRendering();

            commandBuffer.End();

            commandBuffer.Commit();

            GraphicsQueue.Submit();
            GraphicsQueue.WaitIdle();

            SwapChain.Present();
        };

        MainWindow.SizeChanged += (a, b) => SwapChain.Resize();

        MainWindow.Show();

        WindowController.Loop(true);

        CopyQueue.Dispose();
        ComputeQueue.Dispose();
        GraphicsQueue.Dispose();
        ImGuiController.Dispose();
        SwapChain.Dispose();
        Context.Dispose();

        System.Save();
    }

    public static void Exit()
    {
        MainWindow.Close();
    }
}
