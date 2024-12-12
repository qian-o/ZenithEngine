using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.Windowing;
using ZenithEngine.Windowing.Interfaces;

namespace ZenithEngine.Test;

[TestClass]
public class WindowingTest
{
    public const Backend RenderBackend = Backend.Vulkan;

    [TestMethod]
    public void TestCreateWindow()
    {
        AssertEx.IsConsoleErrorEmpty(() =>
        {
            IWindow window = WindowController.CreateWindow();

            window.Show();

            using GraphicsContext context = GraphicsContext.Create(RenderBackend);

            context.CreateDevice(true);

            using CommandProcessor commandProcessor = context.Factory.CreateCommandProcessor();

            SwapChainDesc swapChainDesc = SwapChainDesc.Default(window.Surface);

            using SwapChain swapChain = context.Factory.CreateSwapChain(in swapChainDesc);

            window.Update += (sender, e) =>
            {
            };

            window.Render += (sender, e) =>
            {
                CommandBuffer commandBuffer = commandProcessor.CommandBuffer();

                commandBuffer.Begin();
                commandBuffer.BeginRendering(swapChain.FrameBuffer, new ClearValue(1, new(1)));

                commandBuffer.EndRendering();
                commandBuffer.End();

                commandBuffer.Commit();

                commandProcessor.Submit();
                commandProcessor.WaitIdle();

                swapChain.Present();
            };

            window.SizeChanged += (sender, e) =>
            {
                swapChain.Resize();
            };

            WindowController.Loop(true);
        });
    }
}
