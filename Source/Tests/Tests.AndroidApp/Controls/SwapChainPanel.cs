using Graphics.Core;
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

    void Update();

    void Render();

    void Resize(uint width, uint height);
}

internal sealed class SwapChainPanel : View, ISwapChainPanel
{
    private readonly CommandList _commandList;

    private Swapchain? _swapchain;

    public SwapChainPanel()
    {
        _commandList = Device.Factory.CreateGraphicsCommandList();
    }

    public Context Context => App.Context;

    public GraphicsDevice Device => App.Device;

    #region ISwapChainPanel
    void ISwapChainPanel.CreateSwapChainPanel(IVkSurface surface)
    {
        _swapchain = Device.Factory.CreateSwapchain(new SwapchainDescription(surface, Device.GetBestDepthFormat()));
    }

    void ISwapChainPanel.DestroySwapChainPanel()
    {
        _swapchain?.Dispose();

        _swapchain = null;
    }

    void ISwapChainPanel.Update()
    {
    }

    void ISwapChainPanel.Render()
    {
        if (_swapchain == null)
        {
            return;
        }

        _commandList.Begin();

        _commandList.SetFramebuffer(_swapchain.Framebuffer);
        _commandList.ClearColorTarget(0, RgbaFloat.Red);
        _commandList.ClearDepthStencil(1.0f);

        _commandList.End();

        Device.SubmitCommandsAndSwapBuffers(_commandList, _swapchain);
    }

    void ISwapChainPanel.Resize(uint width, uint height)
    {
        _swapchain?.Resize();
    }
    #endregion
}
