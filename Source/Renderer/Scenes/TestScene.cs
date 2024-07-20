using Graphics.Core;
using Graphics.Vulkan;
using Renderer.Components;

namespace Renderer.Scenes;

internal sealed class TestScene(MainWindow mainWindow) : Scene(mainWindow)
{
    protected override void Initialize()
    {
        SampleCount = TextureSampleCount.Count8;
    }

    protected override void UpdateCore(UpdateEventArgs e)
    {
    }

    protected override void RenderCore(CommandList commandList, Framebuffer framebuffer, RenderEventArgs e)
    {
        commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
        commandList.ClearDepthStencil(1.0f);
    }
}
