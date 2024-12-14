using Hexa.NET.ImGui;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ImGuiWrapper;
using ZenithEngine.Windowing;
using ZenithEngine.Windowing.Interfaces;

namespace Triangle;

internal class Program
{
    private static void Main(string[] _)
    {
        using GraphicsContext context = GraphicsContext.Create(Backend.Vulkan);
        context.CreateDevice(true);

        IWindow window = WindowController.CreateWindow("Triangle");
        window.Show();

        SwapChainDesc swapChainDesc = SwapChainDesc.Default(window.Surface);

        using SwapChain swapChain = context.Factory.CreateSwapChain(in swapChainDesc);

        using CommandProcessor commandProcessor = context.Factory.CreateCommandProcessor();

        using ImGuiController imGuiController = new(context, swapChain.FrameBuffer.Output, window);

        window.Update += (a, b) =>
        {
            imGuiController.Update(b.DeltaTime, window.Size);
        };

        window.Render += (a, b) =>
        {
            ImGui.ShowDemoWindow();

            CommandBuffer commandBuffer = commandProcessor.CommandBuffer();

            commandBuffer.Begin();

            imGuiController.PrepareResources(commandBuffer);

            commandBuffer.BeginRendering(swapChain.FrameBuffer, new(1));

            imGuiController.Render(commandBuffer);

            commandBuffer.EndRendering();

            commandBuffer.End();

            commandBuffer.Commit();

            commandProcessor.Submit();
            commandProcessor.WaitIdle();

            swapChain.Present();
        };

        window.SizeChanged += (a, b) =>
        {
            swapChain.Resize();
        };

        WindowController.Loop(true);
    }
}
