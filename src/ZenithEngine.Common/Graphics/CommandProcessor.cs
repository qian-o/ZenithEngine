using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public abstract class CommandProcessor(GraphicsContext context,
                                       CommandProcessorType type) : GraphicsResource(context)
{
    private readonly Lock @lock = new();
    private readonly List<CommandBuffer> available = [];
    private readonly List<CommandBuffer> execution = [];

    public CommandProcessorType Type { get; } = type;

    public bool CanExecute => execution.Count > 0;

    /// <summary>
    /// Gets the next available command buffer.
    /// </summary>
    /// <returns></returns>
    public CommandBuffer CommandBuffer()
    {
        using Lock.Scope _ = @lock.EnterScope();

        CommandBuffer commandBuffer;

        if (available.Count > 0)
        {
            commandBuffer = available[0];
            commandBuffer.Reset();

            available.RemoveAt(0);
        }
        else
        {
            commandBuffer = CreateCommandBuffer();
        }

        return commandBuffer;
    }

    /// <summary>
    /// Submits the command buffer to be executed by the GPU.
    /// </summary>
    /// <param name="isSyncCopyTasks">
    /// Whether to call the SyncCopyTasks function in the Graphics Context.
    /// </param>
    public void Submit(bool isSyncCopyTasks = true)
    {
        if (isSyncCopyTasks)
        {
            Context.SyncCopyTasks();
        }

        SubmitCommandBuffers([.. execution]);

        available.AddRange(execution);

        execution.Clear();
    }

    /// <summary>
    /// Waits for the GPU to finish executing all commands.
    /// </summary>
    public abstract void WaitIdle();

    /// <summary>
    /// Commits the command buffer to be executed by the GPU.
    /// </summary>
    /// <param name="commandBuffer">The command buffer to commit.</param>
    internal void CommitCommandBuffer(CommandBuffer commandBuffer)
    {
        using Lock.Scope _ = @lock.EnterScope();

        execution.Add(commandBuffer);
    }

    /// <summary>
    /// Creates a new command buffer.
    /// </summary>
    /// <returns></returns>
    protected abstract CommandBuffer CreateCommandBuffer();

    /// <summary>
    /// Submits the command buffers to be executed by the GPU.
    /// </summary>
    /// <param name="commandBuffers">The command buffers to submit.</param>
    protected abstract void SubmitCommandBuffers(CommandBuffer[] commandBuffers);

    protected override void Destroy()
    {
        foreach (CommandBuffer commandBuffer in available)
        {
            commandBuffer.Dispose();
        }

        foreach (CommandBuffer commandBuffer in execution)
        {
            commandBuffer.Dispose();
        }

        available.Clear();
        execution.Clear();
    }
}
