using Hexa.NET.ImGui;
using GraphicsWindow = Graphics.Core.GraphicsWindow;

namespace Graphics.Vulkan;

internal sealed unsafe class ImGuiWindow
{
    private readonly GraphicsWindow _window;
    private readonly GraphicsDevice _graphicsDevice;

    public ImGuiWindow(ImGuiViewport* viewport)
    {
        _window = GraphicsWindow.CreateWindowByVulkan();
        _graphicsDevice = ((RendererUserData*)ImGui.GetMainViewport().RendererUserData)->GetGraphicsDevice();
    }

    public ImGuiWindow(GraphicsWindow window, GraphicsDevice graphicsDevice)
    {
        _window = window;
        _graphicsDevice = graphicsDevice;
    }

    public void Update()
    {
        _window.DoEvents();
    }

    public GraphicsWindow GraphicsWindow => _window;
}
