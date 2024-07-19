using Graphics.Core;
using Graphics.Vulkan;
using Renderer.Components;

namespace Renderer.Scenes;

internal sealed class TestScene(GraphicsDevice graphicsDevice, ImGuiController imGuiController) : Scene(graphicsDevice, imGuiController)
{
    protected override void UpdateCore(UpdateEventArgs e)
    {
        SampleCount = TextureSampleCount.Count8;
    }

    protected override void RenderCore(CommandList commandList, Framebuffer framebuffer, RenderEventArgs e)
    {
        commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
        commandList.ClearDepthStencil(1.0f);
    }
}
