namespace Graphics.Core;

public class RenderEventArgs(float deltaTime) : EventArgs
{
    public float DeltaTime { get; } = deltaTime;
}
