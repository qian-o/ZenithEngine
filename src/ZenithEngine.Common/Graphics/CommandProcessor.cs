using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public abstract class CommandProcessor(GraphicsContext context,
                                       CommandProcessorType type) : GraphicsResource(context)
{
    private readonly Queue<CommandBuffer> available = new();

    private CommandBuffer[] executionArray = new CommandBuffer[64];
    private int executionArraySize;

    public CommandProcessorType Type { get; } = type;

    /// <summary>
    /// Gets the next available command buffer.
    /// </summary>
    /// <returns></returns>
    public CommandBuffer CommandBuffer()
    {
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

        for (int i = 0; i < executionArraySize; i++)
        {
            CommandBuffer commandBuffer = executionArray[i];

            SubmitCommandBuffer(commandBuffer);

            available.Enqueue(commandBuffer);
        }

        Array.Clear(executionArray, 0, executionArraySize);
        executionArraySize = 0;
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
        if (executionArray.Length == executionArraySize)
        {
            Array.Resize(ref executionArray, executionArray.Length + 64);
        }

        executionArray[executionArraySize++] = commandBuffer;
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
            CommandBuffer commandBuffer = available.Dequeue();
            commandBuffer.Dispose();
        }
    }
}
