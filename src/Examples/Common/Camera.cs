using Silk.NET.Maths;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Events;
using ZenithEngine.Windowing.Interfaces;

namespace Common;

public class Camera
{
    private readonly HashSet<Key> keyDowns = [];

    private Vector2D<int>? lastMousePosition;

    public Camera(IWindow window)
    {
        window.KeyDown += Window_KeyDown;
        window.KeyUp += Window_KeyUp;
        window.MouseDown += Window_MouseDown;
        window.MouseMove += Window_MouseMove;
        window.MouseUp += Window_MouseUp;
    }

    public Vector3D<float> Position { get; private set; } = Vector3D<float>.Zero;

    public Vector3D<float> Front { get; private set; } = Vector3D<float>.UnitZ;

    public Vector3D<float> Up { get; private set; } = Vector3D<float>.UnitY;

    public Vector3D<float> Right { get; private set; } = Vector3D<float>.UnitX;

    public float AspectRatio { get; private set; } = 1.0f;

    public float NearPlane { get; set; } = 0.1f;

    public float FarPlane { get; set; } = 1000.0f;

    public float Fov { get; set; } = 45.0f;

    public void Transform(Matrix4X4<float> matrix)
    {
        Position = Vector3D.Transform(Position, matrix);
        Front = Vector3D.TransformNormal(Front, matrix);

        Right = Vector3D.Normalize(Vector3D.Cross(Front, Vector3D<float>.UnitY));
        Up = Vector3D.Normalize(Vector3D.Cross(Right, Front));
    }

    public void Update(double deltaSeconds, Vector2D<uint> size)
    {
        const float speed = 2.5f;

        float deltaTime = (float)deltaSeconds;

        if (keyDowns.Contains(Key.W))
        {
            Position += Front * speed * deltaTime;
        }

        if (keyDowns.Contains(Key.S))
        {
            Position -= Front * speed * deltaTime;
        }

        if (keyDowns.Contains(Key.A))
        {
            Position -= Right * speed * deltaTime;
        }

        if (keyDowns.Contains(Key.D))
        {
            Position += Right * speed * deltaTime;
        }

        if (keyDowns.Contains(Key.Q))
        {
            Position += Up * speed * deltaTime;
        }

        if (keyDowns.Contains(Key.E))
        {
            Position -= Up * speed * deltaTime;
        }

        AspectRatio = size.X / (float)size.Y;
    }

    private void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        keyDowns.Add(e.Key);
    }

    private void Window_KeyUp(object? sender, KeyEventArgs e)
    {
        keyDowns.Remove(e.Key);
    }

    private void Window_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button is MouseButton.Right)
        {
            lastMousePosition = e.Position;
        }
    }

    private void Window_MouseMove(object? sender, ValueEventArgs<Vector2D<int>> e)
    {
        const float clipRadians = 1.553343f;

        if (lastMousePosition.HasValue)
        {
            Vector2D<int> delta = e.Value - lastMousePosition.Value;

            float yaw = -delta.X * 0.01f;
            float pitch = -delta.Y * 0.01f;

            float newPitch = MathF.Asin(Front.Y) + pitch;

            if (newPitch > clipRadians)
            {
                newPitch = clipRadians;
            }
            else if (newPitch < -clipRadians)
            {
                newPitch = -clipRadians;
            }

            pitch = newPitch - MathF.Asin(Front.Y);

            Front = Vector3D.TransformNormal(Front, Matrix4X4.CreateFromAxisAngle(Up, yaw));
            Front = Vector3D.TransformNormal(Front, Matrix4X4.CreateFromAxisAngle(Right, pitch));

            Right = Vector3D.Normalize(Vector3D.Cross(Front, Vector3D<float>.UnitY));
            Up = Vector3D.Normalize(Vector3D.Cross(Right, Front));

            lastMousePosition = e.Value;
        }
    }

    private void Window_MouseUp(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button is MouseButton.Right)
        {
            lastMousePosition = null;
        }
    }
}
