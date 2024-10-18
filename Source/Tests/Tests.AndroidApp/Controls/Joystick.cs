using CommunityToolkit.Maui.Behaviors;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Tests.AndroidApp.Controls;

public class Joystick : SKCanvasView
{
    private bool _enabled;

    public Joystick()
    {
        IgnorePixelScaling = true;
        EnableTouchEvents = true;

        PaintSurface += Joystick_PaintSurface;

        Touch += (a, b) =>
        {
            _enabled = b.ActionType is SKTouchAction.Pressed or SKTouchAction.Moved;

            InvalidateSurface();
        };

        Behaviors.Add(new TouchBehavior());
    }

    private void Joystick_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        SKImageInfo info = e.Info;
        SKCanvas canvas = e.Surface.Canvas;

        float width = info.Width;
        float height = info.Height;

        canvas.Clear();

        DrawOutline(canvas, width, height, 2, SKColors.White);

        DrawInnerCircle(canvas, width, height, 12, new SKColor(102, 204, 255), SKColors.White);

        DrawHandle(canvas, width, height, 24, SKColors.White, _enabled);
    }

    private static void DrawOutline(SKCanvas canvas,
                                    float width,
                                    float height,
                                    float strokeWidth,
                                    SKColor color,
                                    float alpha = 0.5f,
                                    float interval = 4)
    {
        float centerX = width / 2;
        float centerY = height / 2;

        canvas.DrawArc(new SKRect(0, 0, width, height), 0, 360, false, new SKPaint
        {
            IsAntialias = true,
            IsDither = true,
            Color = ColorWithAlpha(color, alpha),
            Style = SKPaintStyle.Fill
        });

        canvas.DrawArc(new SKRect(strokeWidth, strokeWidth, width - strokeWidth, height - strokeWidth), 0, 360, false, new SKPaint
        {
            IsAntialias = true,
            IsDither = true,
            Color = SKColors.White,
            Style = SKPaintStyle.Fill,
            BlendMode = SKBlendMode.DstOut
        });

        canvas.DrawRect(new SKRect(centerX - interval, 0, centerX + interval, height), new SKPaint
        {
            IsAntialias = true,
            IsDither = true,
            Color = SKColors.Red,
            Style = SKPaintStyle.Fill,
            BlendMode = SKBlendMode.DstOut
        });

        canvas.DrawRect(new SKRect(0, centerY - interval, width, centerY + interval), new SKPaint
        {
            IsAntialias = true,
            IsDither = true,
            Color = SKColors.Red,
            Style = SKPaintStyle.Fill,
            BlendMode = SKBlendMode.DstOut
        });
    }

    private static void DrawInnerCircle(SKCanvas canvas,
                                        float width,
                                        float height,
                                        float margin,
                                        SKColor inColor,
                                        SKColor outColor,
                                        float alpha = 0.5f)
    {
        float centerX = width / 2;
        float centerY = height / 2;

        canvas.DrawArc(new SKRect(margin, margin, width - margin, height - margin), 0, 360, false, new SKPaint
        {
            IsAntialias = true,
            IsDither = true,
            Style = SKPaintStyle.Fill,
            Shader = SKShader.CreateRadialGradient(new SKPoint(centerX, centerY),
                                                   centerX,
                                                   [ColorWithAlpha(inColor, alpha), ColorWithAlpha(outColor, 0)],
                                                   [0.5f, 1],
                                                   SKShaderTileMode.Clamp)
        });
    }

    private static void DrawHandle(SKCanvas canvas,
                                   float width,
                                   float height,
                                   float radius,
                                   SKColor color,
                                   bool enabled,
                                   float alpha = 0.8f)
    {
        float centerX = width / 2;
        float centerY = height / 2;

        if (enabled)
        {
            canvas.DrawCircle(centerX, radius + 6, radius, new SKPaint
            {
                IsAntialias = true,
                IsDither = true,
                Color = ColorWithAlpha(color, alpha),
                Style = SKPaintStyle.Fill
            });
        }
        else
        {
            canvas.DrawCircle(centerX, centerY, radius, new SKPaint
            {
                IsAntialias = true,
                IsDither = true,
                Color = ColorWithAlpha(color, alpha),
                Style = SKPaintStyle.Fill
            });
        }
    }

    private static SKColor ColorWithAlpha(SKColor color, float alpha)
    {
        return new SKColor(color.Red, color.Green, color.Blue, (byte)(color.Alpha * alpha));
    }
}
