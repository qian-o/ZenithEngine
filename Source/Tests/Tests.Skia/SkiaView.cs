using System.Numerics;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using SkiaSharp;
using Tests.Core;

namespace Tests.Skia;

internal abstract class SkiaView(string title, GraphicsDevice device, ImGuiController imGuiController) : View(title)
{
    private readonly GRContext _context = SkiaVk.CreateContext(device);

    private Texture? _texture;
    private SKSurface? _surface;

    protected override void OnUpdate(UpdateEventArgs e)
    {
    }

    protected override void OnRender(RenderEventArgs e)
    {
        if (_surface != null)
        {
            using SKCanvas canvas = _surface.Canvas;

            canvas.Clear(SKColors.White);

            OnRenderSurface(canvas, e);

            canvas.Flush();
        }
        if (_texture != null)
        {
            ImGui.Image(imGuiController.GetBinding(device.Factory, _texture),
                        new Vector2(_texture.Width, _texture.Height));
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        _surface?.Dispose();

        if (_texture != null)
        {
            imGuiController.RemoveBinding(imGuiController.GetBinding(device.Factory, _texture));

            _texture.Dispose();
        }

        _texture = device.Factory.CreateTexture(TextureDescription.Texture2D(Width,
                                                                             Height,
                                                                             1,
                                                                             PixelFormat.R8G8B8A8UNorm,
                                                                             TextureUsage.Sampled | TextureUsage.RenderTarget));

        _surface = SkiaVk.CreateSurface(_context, _texture);
    }

    protected abstract void OnRenderSurface(SKCanvas canvas, RenderEventArgs e);

    protected override void Destroy()
    {
        _surface?.Dispose();
        _texture?.Dispose();

        _context.Dispose();
    }
}
