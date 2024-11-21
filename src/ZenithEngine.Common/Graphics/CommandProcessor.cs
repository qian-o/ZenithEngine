using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public abstract class CommandProcessor(GraphicsContext context,
                                       CommandProcessorType type) : GraphicsResource(context)
{
    public CommandProcessorType Type { get; } = type;

    /// <summary>
    /// Gets the next available command buffer.
    /// </summary>
    /// <returns></returns>
    public abstract CommandBuffer CommandBuffer();

    /// <summary>
    /// Submits the command buffer to be executed by the GPU.
    /// </summary>
    /// <param name="isSyncCopyTasks">
    /// Whether to call the SyncCopyTasks function in the Graphics Context.
    /// </param>
    public abstract void Submit(bool isSyncCopyTasks = true);

    /// <summary>
    /// Waits for the GPU to finish executing all commands.
    /// </summary>
    public abstract void WaitIdle();
}
