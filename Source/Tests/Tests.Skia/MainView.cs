using Graphics.Vulkan;
using SkiaSharp;

namespace Tests.Skia;

internal sealed class MainView(GraphicsDevice device, ImGuiController imGuiController) : SkiaView("Skia View", device, imGuiController)
{
    protected override void OnRenderSurface(SKCanvas canvas)
    {
    }
}
