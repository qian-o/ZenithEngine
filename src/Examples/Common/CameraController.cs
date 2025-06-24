using Silk.NET.Maths;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Events;
using ZenithEngine.Windowing.Interfaces;

namespace Common;

public class CameraController
{
    private readonly HashSet<Key> keyDowns = [];
    private readonly Dictionary<GamepadAxis, float> gamepadAxes = new();

    private Vector2D<int>? lastMousePosition;

    public CameraController(IWindow window)
    {
        window.KeyDown += Window_KeyDown;
        window.KeyUp += Window_KeyUp;
        window.MouseDown += Window_MouseDown;
        window.MouseMove += Window_MouseMove;
        window.MouseUp += Window_MouseUp;
        window.GamepadAxisMotion += Window_GamepadAxisMotion;
        
        // Initialize gamepad axes
        foreach (GamepadAxis axis in Enum.GetValues<GamepadAxis>())
        {
            gamepadAxes[axis] = 0.0f;
        }
    }

    public Vector2D<uint> Size { get; private set; } = Vector2D<uint>.One;

    public Vector3D<float> Position { get; private set; } = Vector3D<float>.Zero;

    public Vector3D<float> Forward { get; private set; } = Vector3D<float>.UnitZ;

    public Vector3D<float> Right { get; private set; } = Vector3D<float>.UnitX;

    public Vector3D<float> Up { get; private set; } = Vector3D<float>.UnitY;

    public float NearPlane { get; set; } = 0.1f;

    public float FarPlane { get; set; } = 1000.0f;

    public float Fov { get; set; } = 40.0f;

    public float Speed { get; set; } = 2.5f;

    public float AspectRatio => Size.X / (float)Size.Y;

    public void Transform(Matrix4X4<float> matrix)
    {
        Position = Vector3D.Transform(Position, matrix);
        Forward = Vector3D.TransformNormal(Forward, matrix);

        Right = Vector3D.Normalize(Vector3D.Cross(Forward, Vector3D<float>.UnitY));
        Up = Vector3D.Normalize(Vector3D.Cross(Right, Forward));
    }

    public void Update(double deltaSeconds, Vector2D<uint> size)
    {
        Size = size;

        if (keyDowns.Contains(Key.W))
        {
            Position += Forward * Speed * (float)deltaSeconds;
        }

        if (keyDowns.Contains(Key.S))
        {
            Position -= Forward * Speed * (float)deltaSeconds;
        }

        if (keyDowns.Contains(Key.A))
        {
            Position -= Right * Speed * (float)deltaSeconds;
        }

        if (keyDowns.Contains(Key.D))
        {
            Position += Right * Speed * (float)deltaSeconds;
        }

        if (keyDowns.Contains(Key.Q))
        {
            Position -= Up * Speed * (float)deltaSeconds;
        }

        if (keyDowns.Contains(Key.E))
        {
            Position += Up * Speed * (float)deltaSeconds;
        }

        // Gamepad movement using left stick
        float leftStickX = gamepadAxes[GamepadAxis.LeftX];
        float leftStickY = gamepadAxes[GamepadAxis.LeftY];
        
        // Apply deadzone
        const float deadzone = 0.15f;
        if (Math.Abs(leftStickX) > deadzone)
        {
            Position += Right * leftStickX * Speed * (float)deltaSeconds;
        }
        if (Math.Abs(leftStickY) > deadzone)
        {
            Position -= Forward * leftStickY * Speed * (float)deltaSeconds; // Inverted Y
        }

        // Gamepad camera rotation using right stick
        float rightStickX = gamepadAxes[GamepadAxis.RightX];
        float rightStickY = gamepadAxes[GamepadAxis.RightY];
        
        if (Math.Abs(rightStickX) > deadzone || Math.Abs(rightStickY) > deadzone)
        {
            const float sensitivity = 3.0f;
            float yaw = -rightStickX * sensitivity * (float)deltaSeconds;
            float pitch = rightStickY * sensitivity * (float)deltaSeconds;

            // Clamp pitch to prevent camera flipping
            const float clipRadians = MathF.PI / 2.0f - 0.01f;
            float newPitch = MathF.Asin(Forward.Y) + pitch;

            if (newPitch > clipRadians)
            {
                newPitch = clipRadians;
            }
            else if (newPitch < -clipRadians)
            {
                newPitch = -clipRadians;
            }

            pitch = newPitch - MathF.Asin(Forward.Y);

            Forward = Vector3D.TransformNormal(Forward, Matrix4X4.CreateFromAxisAngle(Up, yaw));
            Forward = Vector3D.TransformNormal(Forward, Matrix4X4.CreateFromAxisAngle(Right, pitch));

            Right = Vector3D.Normalize(Vector3D.Cross(Forward, Vector3D<float>.UnitY));
            Up = Vector3D.Normalize(Vector3D.Cross(Right, Forward));
        }
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
        const float clipRadians = 89.0f * MathF.PI / 180.0f;

        if (lastMousePosition.HasValue)
        {
            float pixelToRadianX = MathF.PI / Size.X;
            float pixelToRadianY = MathF.PI / Size.Y;

            Vector2D<int> delta = e.Value - lastMousePosition.Value;

            float yaw = -(delta.X * pixelToRadianX);
            float pitch = -(delta.Y * pixelToRadianY);

            float newPitch = MathF.Asin(Forward.Y) + pitch;

            if (newPitch > clipRadians)
            {
                newPitch = clipRadians;
            }
            else if (newPitch < -clipRadians)
            {
                newPitch = -clipRadians;
            }

            pitch = newPitch - MathF.Asin(Forward.Y);

            Forward = Vector3D.TransformNormal(Forward, Matrix4X4.CreateFromAxisAngle(Up, yaw));
            Forward = Vector3D.TransformNormal(Forward, Matrix4X4.CreateFromAxisAngle(Right, pitch));

            Right = Vector3D.Normalize(Vector3D.Cross(Forward, Vector3D<float>.UnitY));
            Up = Vector3D.Normalize(Vector3D.Cross(Right, Forward));

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

    private void Window_GamepadAxisMotion(object? sender, GamepadAxisEventArgs e)
    {
        gamepadAxes[e.Axis] = e.Value;
    }
}
