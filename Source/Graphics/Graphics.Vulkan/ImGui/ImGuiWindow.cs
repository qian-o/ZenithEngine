using Hexa.NET.ImGui;
using Silk.NET.SDL;
using Window = Graphics.Core.Window;

namespace Graphics.Vulkan;

internal sealed unsafe class ImGuiWindow
{
    private readonly Window _window;
    private readonly GraphicsDevice _graphicsDevice;

    public ImGuiWindow(ImGuiViewport* viewport)
    {
        _window = Window.CreateWindowByVulkan();
        _graphicsDevice = ((RendererUserData*)ImGui.GetMainViewport().RendererUserData)->GetGraphicsDevice();

        WindowFlags flags = WindowFlags.None;

        if (viewport->Flags.HasFlag(ImGuiViewportFlags.NoTaskBarIcon))
        {
            flags |= WindowFlags.SkipTaskbar;
        }

        if (viewport->Flags.HasFlag(ImGuiViewportFlags.NoDecoration))
        {
            flags |= WindowFlags.Borderless;
        }
        else
        {
            flags |= WindowFlags.Resizable;
        }

        if (viewport->Flags.HasFlag(ImGuiViewportFlags.TopMost))
        {
            flags |= WindowFlags.AlwaysOnTop;
        }

        _window.Resize += (_, _) => viewport->PlatformRequestResize = 1;
        _window.Move += (_, _) => viewport->PlatformRequestMove = 1;
        _window.Close += (_, _) => viewport->PlatformRequestClose = 1;
    }

    public ImGuiWindow(Window window, GraphicsDevice graphicsDevice)
    {
        _window = window;
        _graphicsDevice = graphicsDevice;
    }

    public void Update()
    {
        _window.PollEvents();
    }

    public Window Window => _window;
}
