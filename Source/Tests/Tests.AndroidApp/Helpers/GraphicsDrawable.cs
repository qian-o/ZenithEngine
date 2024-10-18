namespace Tests.AndroidApp.Helpers;

public class GraphicsDrawable(Action<ICanvas, RectF> drawDelegate) : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        drawDelegate(canvas, dirtyRect);
    }
}
