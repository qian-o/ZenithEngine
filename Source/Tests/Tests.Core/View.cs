using System.Numerics;
using Graphics.Core;
using Hexa.NET.ImGui;

namespace Tests.Core;

public abstract class View : DisposableObject
{
    private readonly string _id = Guid.NewGuid().ToString();

    public string Title { get; set; } = string.Empty;

    public uint Width { get; private set; } = 100;

    public uint Height { get; private set; } = 100;

    public void Update(UpdateEventArgs e)
    {
        OnUpdate(e);
    }

    public void Render(RenderEventArgs e)
    {
        ImGui.Begin($"{Title}##{_id}");
        {
            Vector2 size = ImGui.GetContentRegionAvail();
            uint width = Convert.ToUInt32(size.X);
            uint height = Convert.ToUInt32(size.Y);

            if (Width != width || Height != height)
            {
                Width = width;
                Height = height;

                OnResize(new ResizeEventArgs(width, height));
            }

            OnRender(e);
        }
        ImGui.End();
    }

    protected abstract void OnUpdate(UpdateEventArgs e);

    protected abstract void OnRender(RenderEventArgs e);

    protected abstract void OnResize(ResizeEventArgs e);
}
