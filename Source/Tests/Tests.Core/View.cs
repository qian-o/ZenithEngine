using System.Numerics;
using Graphics.Core;
using Graphics.Core.Window;
using Hexa.NET.ImGui;

namespace Tests.Core;

public abstract class View(string title) : DisposableObject
{
    public Vector2 Position { get; private set; }

    public float DpiScale { get; private set; } = 1.0f;

    public bool UseDpiScale { get; set; } = true;

    public uint Width { get; private set; }

    public uint Height { get; private set; }

    public float ActualWidth => UseDpiScale ? Width * (1 / DpiScale) : Width;

    public float ActualHeight => UseDpiScale ? Height * (1 / DpiScale) : Height;

    public void Update(UpdateEventArgs e)
    {
        OnUpdate(e);
    }

    public void Render(RenderEventArgs e)
    {
        ImGui.Begin(title);
        {
            Position = ImGui.GetCursorScreenPos();
            DpiScale = ImGui.GetWindowDpiScale();

            Vector2 size = ImGui.GetContentRegionAvail();

            uint width = Convert.ToUInt32(Math.Max(1, size.X));
            uint height = Convert.ToUInt32(Math.Max(1, size.Y));

            if (Width != width || Height != height)
            {
                Width = width;
                Height = height;

                OnResize(new ResizeEventArgs(width, height));
            }

            OnRender(e);

            ImGui.End();
        }
    }

    protected abstract void OnUpdate(UpdateEventArgs e);

    protected abstract void OnRender(RenderEventArgs e);

    protected abstract void OnResize(ResizeEventArgs e);
}
