using Graphics.Windowing.Enums;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;

namespace Graphics.Windowing.Interfaces;

public interface IWindow
{
    string Title { get; set; }

    WindowState WindowState { get; set; }

    WindowBorder WindowBorder { get; set; }

    Vector2D<int> MinimumSize { get; set; }

    Vector2D<int> MaximumSize { get; set; }

    Vector2D<int> Position { get; set; }

    Vector2D<int> Size { get; set; }

    bool IsVisible { get; set; }

    bool TopMost { get; set; }

    bool ShowInTaskbar { get; set; }

    float Opacity { get; set; }

    bool IsCreated { get; }

    nint Handle { get; }

    float DpiScale { get; }

    bool IsFocused { get; }

    IVkSurface VkSurface { get; }

    void Show();

    void ShowDialog();

    void Close();

    void PollEvents();
}
