using System.Numerics;
using Hexa.NET.ImGui;

namespace Tests.Core;

public class ViewController
{
    private readonly List<Action<ImGuiMouseButton, Vector2>> _mouseDownActions = [];
    private readonly List<Action<ImGuiMouseButton, Vector2>> _mouseUpActions = [];
    private readonly List<Action<Vector2>> _mouseMoveActions = [];
    private readonly List<Action<Vector2, float>> _mouseWheelActions = [];

    private bool isLeftMouseDown;
    private bool isRightMouseDown;
    private bool isMiddleMouseDown;
    private Vector2 _lastMousePosition;

    public ViewController AddMouseDown(Action<ImGuiMouseButton, Vector2> action)
    {
        _mouseDownActions.Add(action);

        return this;
    }

    public ViewController AddMouseUp(Action<ImGuiMouseButton, Vector2> action)
    {
        _mouseUpActions.Add(action);

        return this;
    }

    public ViewController AddMouseMove(Action<Vector2> action)
    {
        _mouseMoveActions.Add(action);

        return this;
    }

    public ViewController AddMouseWheel(Action<Vector2, float> action)
    {
        _mouseWheelActions.Add(action);

        return this;
    }

    public void Update(Vector2 viewPosition, float dpiScale)
    {
        Vector2 mousePosition = ImGui.GetMousePos() - viewPosition;
        mousePosition /= dpiScale;

        if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && !isLeftMouseDown)
        {
            isLeftMouseDown = true;
            foreach (Action<ImGuiMouseButton, Vector2> action in _mouseDownActions)
            {
                action(ImGuiMouseButton.Left, mousePosition);
            }
        }
        else if (!ImGui.IsMouseDown(ImGuiMouseButton.Left) && isLeftMouseDown)
        {
            isLeftMouseDown = false;
            foreach (Action<ImGuiMouseButton, Vector2> action in _mouseUpActions)
            {
                action(ImGuiMouseButton.Left, mousePosition);
            }
        }

        if (ImGui.IsMouseDown(ImGuiMouseButton.Right) && !isRightMouseDown)
        {
            isRightMouseDown = true;
            foreach (Action<ImGuiMouseButton, Vector2> action in _mouseDownActions)
            {
                action(ImGuiMouseButton.Right, mousePosition);
            }
        }
        else if (!ImGui.IsMouseDown(ImGuiMouseButton.Right) && isRightMouseDown)
        {
            isRightMouseDown = false;
            foreach (Action<ImGuiMouseButton, Vector2> action in _mouseUpActions)
            {
                action(ImGuiMouseButton.Right, mousePosition);
            }
        }

        if (ImGui.IsMouseDown(ImGuiMouseButton.Middle) && !isMiddleMouseDown)
        {
            isMiddleMouseDown = true;
            foreach (Action<ImGuiMouseButton, Vector2> action in _mouseDownActions)
            {
                action(ImGuiMouseButton.Middle, mousePosition);
            }
        }
        else if (!ImGui.IsMouseDown(ImGuiMouseButton.Middle) && isMiddleMouseDown)
        {
            isMiddleMouseDown = false;
            foreach (Action<ImGuiMouseButton, Vector2> action in _mouseUpActions)
            {
                action(ImGuiMouseButton.Middle, mousePosition);
            }
        }

        if (_lastMousePosition != mousePosition)
        {
            foreach (Action<Vector2> action in _mouseMoveActions)
            {
                action(mousePosition);
            }

            _lastMousePosition = mousePosition;
        }

        float wheel = ImGui.GetIO().MouseWheel;
        if (wheel != 0)
        {
            foreach (Action<Vector2, float> action in _mouseWheelActions)
            {
                action(mousePosition, wheel);
            }
        }
    }
}
