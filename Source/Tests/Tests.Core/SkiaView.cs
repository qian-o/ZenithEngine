using System.Numerics;
using Graphics.Core;
using Graphics.Core.Window;
using Graphics.Vulkan;
using Graphics.Vulkan.ImGui;
using Graphics.Vulkan.Skia;
using Hexa.NET.ImGui;
using SkiaSharp;

namespace Tests.Core;

public abstract class SkiaView(string title,
                               GraphicsDevice device,
                               ImGuiController imGuiController,
                               GRContext grContext) : View(title)
{
    private readonly CommandList _commandList = device.Factory.CreateGraphicsCommandList();

    private FramebufferObject? _framebufferObject;
    private SKSurface? _surface;

    protected override void OnRender(RenderEventArgs e)
    {
        if (_surface != null)
        {
            SKCanvas canvas = _surface.Canvas;

            canvas.Clear(SKColors.White);

            if (UseDpiScale)
            {
                canvas.SetMatrix(SKMatrix.CreateScale(DpiScale, DpiScale));
            }

            OnRenderSurface(canvas, e);

            canvas.ResetMatrix();
        }

        if (_framebufferObject != null)
        {
            _commandList.Begin();

            _framebufferObject.Present(_commandList);

            _commandList.End();

            device.SubmitCommands(_commandList);

            ImGui.Image(imGuiController.GetBinding(device.Factory, _framebufferObject.PresentTexture), new Vector2(_framebufferObject.Width, _framebufferObject.Height));
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        _surface?.Dispose();

        if (_framebufferObject != null)
        {
            imGuiController.RemoveBinding(imGuiController.GetBinding(device.Factory, _framebufferObject.PresentTexture));

            _framebufferObject.Dispose();
        }

        _framebufferObject = new FramebufferObject(device, (int)e.Width, (int)e.Height, TextureSampleCount.Count1);

        _surface = SkiaGraphics.CreateSurface(grContext, _framebufferObject.ColorTexture);
    }

    protected abstract void OnRenderSurface(SKCanvas canvas, RenderEventArgs e);

    protected override void Destroy()
    {
        _surface?.Dispose();
        _framebufferObject?.Dispose();

        _commandList.Dispose();
    }
}
