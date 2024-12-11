using Silk.NET.Vulkan;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKCommandProcessor : CommandProcessor
{
    private readonly VkQueue queue;

    public VKCommandProcessor(GraphicsContext context,
                              CommandProcessorType type) : base(context, type)
    {
        queue = Context.FindQueue(type);
    }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public override void WaitIdle()
    {
        Context.Vk.QueueWaitIdle(queue).ThrowIfError();
    }

    protected override CommandBuffer CreateCommandBuffer()
    {
        return new VKCommandBuffer(Context, this);
    }

    protected override void SubmitCommandBuffer(CommandBuffer commandBuffer)
    {
        fixed (VkCommandBuffer* pCommandBuffer = &commandBuffer.VK().CommandBuffer)
        {
            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = pCommandBuffer
            };

            Context.Vk.QueueSubmit(queue, 1, &submitInfo, default).ThrowIfError();
        }
    }

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.Queue, (ulong)queue.Handle, name);
    }
}
