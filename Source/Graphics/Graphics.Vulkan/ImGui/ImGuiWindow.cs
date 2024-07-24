using Graphics.Core;
using Hexa.NET.ImGui;
using Silk.NET.Windowing;

namespace Graphics.Vulkan;

internal sealed unsafe class ImGuiWindow : DisposableObject
{
    private readonly ImGuiViewport* _viewport;
    private readonly GraphicsWindow _graphicsWindow;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly bool _isExernalWindow;

    private Swapchain? _swapchain;

    public ImGuiWindow(ImGuiViewport* viewport)
    {
        _viewport = viewport;
        _graphicsWindow = GraphicsWindow.CreateWindowByVulkan();
        _graphicsDevice = ((RendererUserData*)ImGui.GetMainViewport().RendererUserData)->GetGraphicsDevice();
        _isExernalWindow = false;
    }

    public ImGuiWindow(GraphicsWindow window, GraphicsDevice graphicsDevice)
    {
        _graphicsWindow = window;
        _graphicsDevice = graphicsDevice;
        _isExernalWindow = true;
    }

    public void Show()
    {
        if (_isExernalWindow)
        {
            return;
        }

        _graphicsWindow.Show();

        Initialize();
    }

    public void Close()
    {
        if (_isExernalWindow)
        {
            return;
        }

        _graphicsWindow.Exit();
    }

    public void DoEvents()
    {
        if (_isExernalWindow)
        {
            return;
        }

        _graphicsWindow.DoEvents();
    }

    public GraphicsWindow GraphicsWindow => _graphicsWindow;

    public Swapchain? Swapchain => _swapchain;

    protected override void Destroy()
    {
        if (_isExernalWindow)
        {
            return;
        }

        _swapchain?.Dispose();
        _graphicsWindow.Dispose();
    }

    private void Initialize()
    {
        if (_viewport->Flags.HasFlag(ImGuiViewportFlags.NoTaskBarIcon))
        {
            _graphicsWindow.ShowInTaskbar = false;
        }

        if (_viewport->Flags.HasFlag(ImGuiViewportFlags.NoDecoration))
        {
            _graphicsWindow.WindowBorder = WindowBorder.Hidden;
        }
        else
        {
            _graphicsWindow.WindowBorder = WindowBorder.Resizable;
        }

        if (_viewport->Flags.HasFlag(ImGuiViewportFlags.TopMost))
        {
            _graphicsWindow.TopMost = true;
        }

        SwapchainDescription swapchainDescription = new(_graphicsWindow.VkSurface!, (uint)_graphicsWindow.FramebufferWidth, (uint)_graphicsWindow.FramebufferHeight, _graphicsDevice.GetBestDepthFormat());

        _swapchain = new(_graphicsDevice, ref swapchainDescription);

        _graphicsWindow.Resize += (_, e) =>
        {
            _swapchain.Resize(e.Width, e.Height);
        };
    }
}
