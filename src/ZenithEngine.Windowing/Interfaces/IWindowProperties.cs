using Silk.NET.Maths;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.Windowing.Interfaces;

public interface IWindowProperties
{
    ISurface Surface { get; }

    string Title { get; set; }

    WindowState State { get; set; }

    WindowBorder Border { get; set; }

    bool TopMost { get; set; }

    bool ShowInTaskbar { get; set; }

    Vector2D<int> Position { get; set; }

    Vector2D<uint> Size { get; set; }

    Vector2D<uint> MinimumSize { get; set; }

    Vector2D<uint> MaximumSize { get; set; }

    float Opacity { get; set; }

    double UpdatePerSecond { get; set; }

    double RenderPerSecond { get; set; }
}
