namespace Graphics.Engine;

public abstract class CommandBuffer(Context context) : DeviceResource(context)
{
    public abstract void Begin();

    public void End()
    {
        EndInternal();
    }

    protected abstract void EndInternal();
}
