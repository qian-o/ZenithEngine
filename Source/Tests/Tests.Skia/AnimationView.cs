using Graphics.Core;
using Graphics.Vulkan;
using SkiaSharp;
using SkiaSharp.Skottie;

namespace Tests.Skia;

internal sealed class AnimationView(string filePath, GraphicsDevice device, ImGuiController imGuiController) : SkiaView("Skia View", device, imGuiController)
{
    private readonly Animation _animation = Animation.Parse(File.ReadAllText(filePath))!;

    protected override void OnRenderSurface(SKCanvas canvas, RenderEventArgs e)
    {
        _animation.Render(canvas, new SKRect(0, 0, Width, Height));

        _animation.SeekFrameTime(e.TotalTime);
    }

    protected override void Destroy()
    {
        _animation.Dispose();

        base.Destroy();
    }
}
