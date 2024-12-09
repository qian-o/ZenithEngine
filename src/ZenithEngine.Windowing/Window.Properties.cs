using Silk.NET.Maths;
using ZenithEngine.Common.Interfaces;
using ZenithEngine.Windowing.Enums;
using ZenithEngine.Windowing.Interfaces;

namespace ZenithEngine.Windowing;

internal unsafe partial class Window : IWindowProperties
{
    private ISurface? surface = null;
    private Vector2D<int> position = new(20, 20);
    private Vector2D<int> size = new(800, 600);
    private Vector2D<int> minimumSize = new(100, 100);
    private Vector2D<int> maximumSize = new(10000, 10000);

    public ISurface Surface
    {
        get
        {
            if (!IsInitialized())
            {
                throw new InvalidOperationException("The window is not initialized.");
            }

            return surface ??= new Surface(NativeWindow);
        }
    }

    public string Title { get; set; } = "Window";

    public WindowState State { get; set; } = WindowState.Normal;

    public WindowBorder Border { get; set; } = WindowBorder.Resizable;

    public bool TopMost { get; set; } = false;

    public bool ShowInTaskbar { get; set; } = true;

    public Vector2D<int> Position { get => position; set => position = value; }

    public Vector2D<int> Size { get => size; set => size = value; }

    public Vector2D<int> MinimumSize { get => minimumSize; set => minimumSize = value; }

    public Vector2D<int> MaximumSize { get => maximumSize; set => maximumSize = value; }

    public float Opacity { get; set; } = 1.0f;

    public double UpdatePerSecond { get; set; } = 1.0 / 1000.0;

    public double RenderPerSecond { get; set; } = 1.0 / 1000.0;
}
