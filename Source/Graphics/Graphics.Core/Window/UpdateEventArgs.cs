namespace Graphics.Core.Window;

public class UpdateEventArgs(float deltaTime, float totalTime) : EventArgs
{
    public float DeltaTime { get; } = deltaTime;

    public float TotalTime { get; } = totalTime;
}
