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
            using GraphicsContext context = GraphicsContext.Create(RenderBackend);

            context.CreateDevice(true);

            SwapChain swapChain = null!;

            IWindow window = WindowController.CreateWindow();

            window.Loaded += (sender, e) =>
            {
                SwapChainDesc swapChainDesc = SwapChainDesc.Default(window.Surface);

                swapChain = context.Factory.CreateSwapChain(in swapChainDesc);
            };

            window.Unloaded += (sender, e) =>
            {
                swapChain.Dispose();
            };

            window.Update += (sender, e) =>
            {
            };

            window.Render += (sender, e) =>
            {
            };

            window.SizeChanged += (sender, e) =>
            {
                swapChain.Resize();
            };

            window.Show();

            WindowController.Loop();
        });
    }
}
