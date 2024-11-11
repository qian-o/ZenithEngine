namespace Graphics.Engine;

public abstract class CommandBuffer(Context context) : DeviceResource(context)
{
    /// <summary>
    /// Sets the initial state for this command buffer. This function must be called
    /// before other graphics commands can be issued.
    /// </summary>
    public abstract void Begin();

    /// <summary>
    /// Completes the command buffer.
    /// </summary>
    public void End()
    {
        ClearCache();
        EndInternal();
    }

    /// <summary>
    /// Resets the command buffer to the initial state.
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// Commits this command buffer to the command queue.
    /// </summary>
    public abstract void Commit();

    /// <summary>
    /// Clears all cached values of this command buffer.
    /// </summary>
    protected abstract void ClearCache();

    /// <summary>
    /// Finalizes the command buffer.
    /// </summary>
    protected abstract void EndInternal();
}
