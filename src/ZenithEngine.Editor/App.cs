using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.Editor.Models;
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
    }

    public static SystemConfig System { get; }

    public static GraphicsContext Context { get; }

    public static SwapChain SwapChain { get; private set; } = null!;

    public static ImGuiController ImGuiController { get; private set; } = null!;

    public static CommandProcessor GraphicsQueue { get; private set; } = null!;

    public static CommandProcessor ComputeQueue { get; private set; } = null!;

    public static CommandProcessor CopyQueue { get; private set; } = null!;

    public static void Run()
    {
        IWindow window = WindowController.CreateWindow("ZenithEngine Editor", 1280, 720);

        window.Loaded += (_, _) =>
        {
            window.Center();

            SwapChainDesc swapChainDesc = new() { Surface = window.Surface };

            SwapChain = Context.Factory.CreateSwapChain(in swapChainDesc);

            ImGuiController = new(window, Context, SwapChain.FrameBuffer.Output);

            GraphicsQueue = Context.Factory.CreateCommandProcessor(CommandProcessorType.Graphics);

            ComputeQueue = Context.Factory.CreateCommandProcessor(CommandProcessorType.Compute);

            CopyQueue = Context.Factory.CreateCommandProcessor(CommandProcessorType.Copy);
        };

        window.Update += (_, args) => ImGuiController.Update(args.DeltaTime, window.Size);

        window.Render += (_, _) =>
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

        window.SizeChanged += (a, b) => SwapChain.Resize();

        window.Show();

        WindowController.Loop(true);

        CopyQueue.Dispose();
        ComputeQueue.Dispose();
        GraphicsQueue.Dispose();
        ImGuiController.Dispose();
        SwapChain.Dispose();
        Context.Dispose();

        System.Save();
    }
}
