using Graphics.Vulkan;
using Silk.NET.Core.Contexts;

namespace Tests.AndroidApp.Controls;

internal interface ISwapChainPanel
{
    Context Context { get; }

    void CreateSwapChainPanel(IVkSurface surface);

    void DestroySwapChainPanel();
}

internal sealed class SwapChainPanel : View, ISwapChainPanel
{
    public Context Context => App.Context;

    public void CreateSwapChainPanel(IVkSurface surface)
    {
    }

    public void DestroySwapChainPanel()
    {
    }
}
