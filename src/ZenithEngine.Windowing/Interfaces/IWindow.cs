using Silk.NET.Maths;
using ZenithEngine.Common.Interfaces;
using ZenithEngine.Windowing.Enums;
using ZenithEngine.Windowing.Events;

namespace ZenithEngine.Windowing.Interfaces;

public interface IWindow
{
    event EventHandler<EventArgs>? Loaded;

    event EventHandler<TimeEventArgs>? Update;

    event EventHandler<TimeEventArgs>? Render;

    event EventHandler<ValueEventArgs<WindowState>>? StateChanged;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? PositionChanged;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? SizeChanged;

    event EventHandler<KeyEventArgs>? KeyDown;

    event EventHandler<KeyEventArgs>? KeyUp;

    event EventHandler<ValueEventArgs<char>>? KeyChar;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseMove;

    event EventHandler<MouseButtonEventArgs>? MouseDown;

    event EventHandler<MouseButtonEventArgs>? MouseUp;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseWheel;

    event EventHandler<MouseButtonEventArgs>? Click;

    event EventHandler<MouseButtonEventArgs>? DoubleClick;

    event EventHandler<EventArgs>? Unloaded;

    string Title { get; set; }

    WindowState State { get; set; }

    WindowBorder Border { get; set; }

    bool ShowInTaskbar { get; set; }

    Vector2D<int> Position { get; set; }

    Vector2D<int> Size { get; set; }

    Vector2D<int> MinimumSize { get; set; }

    Vector2D<int> MaximumSize { get; set; }

    double UpdatePerSecond { get; set; }

    double RenderPerSecond { get; set; }

    bool TopMost { get; set; }

    float Opacity { get; set; }

    double Time { get; }

    bool IsFocused { get; }

    float DpiScale { get; }

    ISurface Surface { get; }

    void Show();

    void Close();

    void Focus();

    void DoEvents();

    void DoUpdate();

    void DoRender();
}
