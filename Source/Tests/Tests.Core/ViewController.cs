using System.Numerics;
using Hexa.NET.ImGui;

namespace Tests.Core;

public class MouseButtonEventArgs(ImGuiMouseButton button, Vector2 position) : EventArgs
{
    public ImGuiMouseButton Button { get; } = button;

    public Vector2 Position { get; } = position;
}

public class MouseMoveEventArgs(Vector2 position) : EventArgs
{
    public Vector2 Position { get; } = position;
}

public class MouseWheelEventArgs(Vector2 position, float wheel) : EventArgs
{
    public Vector2 Position { get; } = position;

    public float Wheel { get; } = wheel;
}

public class ViewController(View view)
{
    public event EventHandler<MouseButtonEventArgs>? MouseDown;
    public event EventHandler<MouseButtonEventArgs>? MouseUp;
    public event EventHandler<MouseMoveEventArgs>? MouseMove;
    public event EventHandler<MouseWheelEventArgs>? MouseWheel;

    private bool isLeftMouseDown;
    private bool isRightMouseDown;
    private bool isMiddleMouseDown;
    private Vector2 lastMousePosition;

    public bool UseDpiScale { get; set; } = true;

    public void Update()
    {
        Vector2 mousePosition = ImGui.GetMousePos() - view.Position;
        mousePosition /= view.DpiScale;

        if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && !isLeftMouseDown)
        {
            isLeftMouseDown = true;

            MouseDown?.Invoke(view, new MouseButtonEventArgs(ImGuiMouseButton.Left, mousePosition));
        }
        else if (!ImGui.IsMouseDown(ImGuiMouseButton.Left) && isLeftMouseDown)
        {
            isLeftMouseDown = false;

            MouseUp?.Invoke(view, new MouseButtonEventArgs(ImGuiMouseButton.Left, mousePosition));
        }

        if (ImGui.IsMouseDown(ImGuiMouseButton.Right) && !isRightMouseDown)
        {
            isRightMouseDown = true;

            MouseDown?.Invoke(view, new MouseButtonEventArgs(ImGuiMouseButton.Right, mousePosition));
        }
        else if (!ImGui.IsMouseDown(ImGuiMouseButton.Right) && isRightMouseDown)
        {
            isRightMouseDown = false;

            MouseUp?.Invoke(view, new MouseButtonEventArgs(ImGuiMouseButton.Right, mousePosition));
        }

        if (ImGui.IsMouseDown(ImGuiMouseButton.Middle) && !isMiddleMouseDown)
        {
            isMiddleMouseDown = true;

            MouseDown?.Invoke(view, new MouseButtonEventArgs(ImGuiMouseButton.Middle, mousePosition));
        }
        else if (!ImGui.IsMouseDown(ImGuiMouseButton.Middle) && isMiddleMouseDown)
        {
            isMiddleMouseDown = false;

            MouseUp?.Invoke(view, new MouseButtonEventArgs(ImGuiMouseButton.Middle, mousePosition));
        }

        if (lastMousePosition != mousePosition)
        {
            MouseMove?.Invoke(view, new MouseMoveEventArgs(mousePosition));

            lastMousePosition = mousePosition;
        }

        float wheel = ImGui.GetIO().MouseWheel;
        if (wheel != 0)
        {
            MouseWheel?.Invoke(view, new MouseWheelEventArgs(mousePosition, wheel));
        }
    }
}
