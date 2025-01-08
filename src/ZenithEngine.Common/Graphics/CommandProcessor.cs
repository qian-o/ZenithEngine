using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public abstract class CommandProcessor(GraphicsContext context,
                                       CommandProcessorType type) : GraphicsResource(context)
{
    private readonly Lock @lock = new();
    private readonly Queue<CommandBuffer> available = new();

    private CommandBuffer[] execution = new CommandBuffer[64];
    private uint executionLength;

    public CommandProcessorType Type { get; } = type;

    public bool CanExecute => executionLength > 0;

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
            commandBuffer = available.Dequeue();
            commandBuffer.Reset();
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

        for (uint i = 0; i < executionLength; i++)
        {
            CommandBuffer commandBuffer = execution[i];

            SubmitCommandBuffer(commandBuffer);

            available.Enqueue(commandBuffer);
        }

        Array.Clear(execution, 0, (int)executionLength);
        executionLength = 0;
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

        if (execution.Length == executionLength)
        {
            Array.Resize(ref execution, execution.Length + 64);
        }

        execution[executionLength++] = commandBuffer;
    }

    /// <summary>
    /// Creates a new command buffer.
    /// </summary>
    /// <returns></returns>
    protected abstract CommandBuffer CreateCommandBuffer();

    /// <summary>
    /// Submits the command buffer to be executed by the GPU.
    /// </summary>
    /// <param name="commandBuffer">The command buffer to submit.</param>
    protected abstract void SubmitCommandBuffer(CommandBuffer commandBuffer);

    protected override void Destroy()
    {
        while (available.Count > 0)
        {
            available.Dequeue().Dispose();
        }
    }
}
