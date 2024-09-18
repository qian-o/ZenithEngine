﻿using Graphics.Core;
using Graphics.Vulkan;
using SkiaSharp;
using SkiaSharp.Skottie;

namespace Tests.Skia;

internal sealed class AnimationView(string filePath,
                                    GraphicsDevice device,
                                    ImGuiController imGuiController,
                                    GRContext grContext) : SkiaView("Skia View", device, imGuiController, grContext)
{
    private readonly Animation _animation = Animation.Parse(File.ReadAllText(filePath))!;

    private float _time;

    protected override void OnRenderSurface(SKCanvas canvas, RenderEventArgs e)
    {
        _animation.Render(canvas, new SKRect(0, 0, Width, Height));

        _animation.SeekFrameTime(_time += e.DeltaTime);

        if (_time >= _animation.Duration.TotalSeconds)
        {
            _time = 0;
        }
    }

    protected override void Destroy()
    {
        _animation.Dispose();

        base.Destroy();
    }
}
