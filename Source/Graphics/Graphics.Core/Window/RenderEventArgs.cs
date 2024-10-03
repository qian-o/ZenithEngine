namespace Graphics.Core;

public class RenderEventArgs(float deltaTime, float totalTime) : EventArgs
{
    public float DeltaTime { get; } = deltaTime;

    public float TotalTime { get; } = totalTime;
}
