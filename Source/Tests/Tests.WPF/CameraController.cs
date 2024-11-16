using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tests.Core.Helpers;

namespace Tests.WPF;

public class CameraController
{
    private readonly Control control;

    private Vector2? lastMousePosition;
    private bool wDown;
    private bool sDown;
    private bool aDown;
    private bool dDown;
    private bool qDown;
    private bool eDown;

    public CameraController(Control control)
    {
        control.Focusable = true;
        control.MouseDown += Control_MouseDown;
        control.MouseUp += Control_MouseUp;
        control.MouseMove += Control_MouseMove;
        control.KeyDown += Control_KeyDown;
        control.KeyUp += Control_KeyUp;

        this.control = control;
    }

    public void Update(float deltaTime)
    {
        const float speed = 0.5f;

        if (wDown)
        {
            Position += Forward * deltaTime * speed;
        }

        if (sDown)
        {
            Position -= Forward * deltaTime * speed;
        }

        if (aDown)
        {
            Position -= Right * deltaTime * speed;
        }

        if (dDown)
        {
            Position += Right * deltaTime * speed;
        }

        if (qDown)
        {
            Position -= Up * deltaTime * speed;
        }

        if (eDown)
        {
            Position += Up * deltaTime * speed;
        }
    }

    private void Control_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Right)
        {
            Point position = e.GetPosition(control);

            lastMousePosition = new Vector2((float)position.X, (float)position.Y);
        }

        control.Focus();
    }

    private void Control_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Right)
        {
            lastMousePosition = null;
        }
    }

    private void Control_MouseMove(object sender, MouseEventArgs e)
    {
        if (lastMousePosition.HasValue)
        {
            Point position = e.GetPosition(control);

            Vector2 pos = new((float)position.X, (float)position.Y);

            Vector2 delta = pos - lastMousePosition.Value;

            float yaw = -delta.X * 0.01f;
            float pitch = -delta.Y * 0.01f;

            float newPitch = MathF.Asin(Forward.Y) + pitch;

            float clipRadians = DegToRad(89.0f);
            if (newPitch > clipRadians)
            {
                newPitch = clipRadians;
            }
            else if (newPitch < -clipRadians)
            {
                newPitch = -clipRadians;
            }

            pitch = newPitch - MathF.Asin(Forward.Y);

            Forward = Vector3.TransformNormal(Forward, Matrix4x4.CreateFromAxisAngle(Up, yaw));
            Forward = Vector3.TransformNormal(Forward, Matrix4x4.CreateFromAxisAngle(Right, pitch));

            Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Forward));

            lastMousePosition = pos;
        }
    }

    private void Control_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.W:
                wDown = true;
                break;
            case Key.S:
                sDown = true;
                break;
            case Key.A:
                aDown = true;
                break;
            case Key.D:
                dDown = true;
                break;
            case Key.Q:
                qDown = true;
                break;
            case Key.E:
                eDown = true;
                break;
        }
    }

    private void Control_KeyUp(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.W:
                wDown = false;
                break;
            case Key.S:
                sDown = false;
                break;
            case Key.A:
                aDown = false;
                break;
            case Key.D:
                dDown = false;
                break;
            case Key.Q:
                qDown = false;
                break;
            case Key.E:
                eDown = false;
                break;
        }
    }

    public Vector3 Position { get; set; } = Vector3.Zero;

    public Vector3 Forward { get; set; } = Vector3.UnitZ;

    public Vector3 Right { get; set; } = Vector3.UnitX;

    public Vector3 Up { get; set; } = Vector3.UnitY;

    public float NearPlane { get; set; } = 0.1f;

    public float FarPlane { get; set; } = 1000.0f;

    public float Fov { get; set; } = 45.0f;

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Position + Forward, Up);

    public Matrix4x4 ProjectionMatrix(double width, double height)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(Fov.ToRadians(), (float)(width / height), NearPlane, FarPlane);
    }

    public void Transform(Matrix4x4 matrix)
    {
        Position = Vector3.Transform(Position, matrix);
        Forward = Vector3.TransformNormal(Forward, matrix);

        Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Forward));
    }

    private static float DegToRad(float degrees)
    {
        return degrees * MathF.PI / 180.0f;
    }
}
