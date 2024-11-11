using Graphics.Engine.Enums;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKCommandProcessor : CommandProcessor
{
    private readonly Queue<VKCommandBuffer> availableBuffers = [];

    private VKCommandBuffer[] waitSubmitBuffers = [];
    private int waitSubmitBufferCount;

    public VKCommandProcessor(Context context, CommandProcessorType type) : base(context)
    {
        FamilyIndex = type switch
        {
            CommandProcessorType.Graphics => Context.GraphicsFamilyIndex,
            CommandProcessorType.Compute => Context.ComputeFamilyIndex,
            CommandProcessorType.Transfer => Context.TransferFamilyIndex,
            _ => throw new NotSupportedException()
        };

        Queue = type switch
        {
            CommandProcessorType.Graphics => Context.GraphicsQueue,
            CommandProcessorType.Compute => Context.ComputeQueue,
            CommandProcessorType.Transfer => Context.TransferQueue,
            _ => throw new NotSupportedException()
        };
    }

    public new VKContext Context => (VKContext)base.Context;

    public uint FamilyIndex { get; }

    public VkQueue Queue { get; }

    public override CommandBuffer CommandBuffer()
    {
        VKCommandBuffer commandBuffer;

        if (availableBuffers.Count == 0)
        {
            commandBuffer = new VKCommandBuffer(Context, this);
        }
        else
        {
            commandBuffer = availableBuffers.Dequeue();
            commandBuffer.Reset();
        }

        return commandBuffer;
    }

    public override void Submit()
    {
        for (int i = 0; i < waitSubmitBufferCount; i++)
        {
            VKCommandBuffer vKCommandBuffer = waitSubmitBuffers[i];

            VkCommandBuffer commandBuffer = vKCommandBuffer.CommandBuffer;
            PipelineStageFlags pipelineStageFlags = PipelineStageFlags.ColorAttachmentOutputBit;

            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = &commandBuffer,
                PWaitDstStageMask = &pipelineStageFlags
            };

            Context.Vk.QueueSubmit(Queue, 1, &submitInfo, default);

            availableBuffers.Enqueue(vKCommandBuffer);
        }

        Array.Clear(waitSubmitBuffers, 0, waitSubmitBufferCount);
        waitSubmitBufferCount = 0;
    }

    public override void WaitIdle()
    {
        Context.Vk.QueueWaitIdle(Queue);
    }

    public void CommitCommandBuffer(VKCommandBuffer commandBuffer)
    {
        if (waitSubmitBuffers.Length == waitSubmitBufferCount)
        {
            Array.Resize(ref waitSubmitBuffers, waitSubmitBuffers.Length + 60);
        }

        waitSubmitBuffers[waitSubmitBufferCount++] = commandBuffer;
    }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.Queue, (ulong)Queue.Handle, name);
    }

    protected override void Destroy()
    {
        foreach (VKCommandBuffer commandBuffer in availableBuffers)
        {
            commandBuffer.Dispose();
        }
    }
}
