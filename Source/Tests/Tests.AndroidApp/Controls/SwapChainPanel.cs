using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Silk.NET.Core.Contexts;

namespace Tests.AndroidApp.Controls;

internal interface ISwapChainPanel
{
    Context Context { get; }

    GraphicsDevice Device { get; }

    void CreateSwapChainPanel(IVkSurface surface);

    void DestroySwapChainPanel();
}

internal sealed class SwapChainPanel : View, ISwapChainPanel
{
    private Swapchain? _swapchain;

    public Context Context => App.Context;

    public GraphicsDevice Device => App.Device;

    public void CreateSwapChainPanel(IVkSurface surface)
    {
        _swapchain?.Dispose();

        _swapchain = Device.Factory.CreateSwapchain(new SwapchainDescription(surface, Device.GetBestDepthFormat()));
    }

    public void DestroySwapChainPanel()
    {
    }
}
