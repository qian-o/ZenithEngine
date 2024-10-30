namespace Graphics.Windowing.Events;

public class TimeEventArgs(float deltaTime, float totalTime) : EventArgs
{
    public float DeltaTime { get; } = deltaTime;

    public float TotalTime { get; } = totalTime;
}
