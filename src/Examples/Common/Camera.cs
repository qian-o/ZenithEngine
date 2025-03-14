using Silk.NET.Maths;
using ZenithEngine.Common.Events;
using ZenithEngine.Windowing.Interfaces;

namespace Common;

public class Camera
{
    public Camera(IWindow window)
    {
        window.MouseDown += Window_MouseDown;
        window.MouseMove += Window_MouseMove;
        window.MouseUp += Window_MouseUp;
    }

    public Vector3D<float> Position { get; private set; } = Vector3D<float>.Zero;

    public Vector3D<float> Front { get; private set; } = Vector3D<float>.UnitZ;

    public Vector3D<float> Up { get; private set; } = Vector3D<float>.UnitY;

    public Vector3D<float> Right { get; private set; } = Vector3D<float>.UnitX;

    public float NearPlane { get; private set; } = 0.1f;

    public float FarPlane { get; private set; } = 1000.0f;

    public float Fov { get; private set; } = 45.0f;

    public float AspectRatio { get; private set; } = 1.0f;

    private void Window_MouseDown(object? sender, MouseButtonEventArgs e)
    {
    }

    private void Window_MouseMove(object? sender, ValueEventArgs<Vector2D<int>> e)
    {
    }

    private void Window_MouseUp(object? sender, MouseButtonEventArgs e)
    {
    }
}
