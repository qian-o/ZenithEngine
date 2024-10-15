using Graphics.Core.Window;
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

    void Update(float deltaTime, float totalTime);

    void Render(float deltaTime, float totalTime);

    void Resize(uint width, uint height);
}

internal sealed class SwapChainPanel : View, ISwapChainPanel
{
    private bool _initialized;

    public event EventHandler? Initialized;

    public event EventHandler<UpdateEventArgs>? Update;

    public event EventHandler<RenderEventArgs>? Render;

    public event EventHandler<ResizeEventArgs>? Resize;

    public event EventHandler? Disposed;

    private Swapchain? _swapchain;

    public Swapchain Swapchain => _swapchain ?? throw new InvalidOperationException("Swapchain is not created.");

    ~SwapChainPanel()
    {
        Disposed?.Invoke(this, EventArgs.Empty);
    }

    #region ISwapChainPanel
    Context ISwapChainPanel.Context => App.Context;

    GraphicsDevice ISwapChainPanel.Device => App.Device;

    void ISwapChainPanel.CreateSwapChainPanel(IVkSurface surface)
    {
        _swapchain = App.Device.Factory.CreateSwapchain(new SwapchainDescription(surface, App.Device.GetBestDepthFormat()));

        if (!_initialized)
        {
            _initialized = true;

            Initialized?.Invoke(this, EventArgs.Empty);
        }
    }

    void ISwapChainPanel.DestroySwapChainPanel()
    {
        _swapchain?.Dispose();

        _swapchain = null;
    }

    void ISwapChainPanel.Update(float deltaTime, float totalTime)
    {
        Update?.Invoke(this, new UpdateEventArgs(deltaTime, totalTime));
    }

    void ISwapChainPanel.Render(float deltaTime, float totalTime)
    {
        if (_swapchain is null)
        {
            return;
        }

        Render?.Invoke(this, new RenderEventArgs(deltaTime, totalTime));
    }

    void ISwapChainPanel.Resize(uint width, uint height)
    {
        _swapchain?.Resize();

        Resize?.Invoke(this, new ResizeEventArgs(width, height));
    }
    #endregion
}
