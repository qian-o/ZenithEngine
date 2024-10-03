using Graphics.Core.Window;
using Graphics.Vulkan;
using Graphics.Vulkan.ImGui;
using SkiaSharp;
using SkiaSharp.Skottie;
using Tests.Core;

namespace Tests.Skia;

internal sealed class AnimationView(string filePath,
                                    GraphicsDevice device,
                                    ImGuiController imGuiController,
                                    GRContext grContext) : SkiaView(Path.GetFileName(filePath), device, imGuiController, grContext)
{
    private readonly Animation _animation = Animation.Parse(File.ReadAllText(filePath))!;

    private float _time;

    protected override void OnUpdate(UpdateEventArgs e)
    {
        _animation.SeekFrameTime(_time += e.DeltaTime);

        if (_time >= _animation.Duration.TotalSeconds)
        {
            _time = 0;
        }
    }

    protected override void OnRenderSurface(SKCanvas canvas, RenderEventArgs e)
    {
        _animation.Render(canvas, new SKRect(0, 0, ActualWidth, ActualHeight));
    }

    protected override void Destroy()
    {
        _animation.Dispose();

        base.Destroy();
    }
}
