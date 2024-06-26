namespace Graphics.Core;

public class UpdateEventArgs(float deltaTime) : EventArgs
{
    public float DeltaTime { get; } = deltaTime;
}
