using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKCommandProcessor : CommandProcessor
{
    private readonly VkQueue queue;

    public VKCommandProcessor(GraphicsContext context,
                              CommandProcessorType type) : base(context, type)
    {
        queue = type switch
        {
            CommandProcessorType.Graphics => Context.GraphicsQueue,
            CommandProcessorType.Compute => Context.ComputeQueue,
            CommandProcessorType.Copy => Context.CopyQueue,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(type))
        };
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

    protected override void SubmitCommandBuffers(CommandBuffer[] commandBuffers)
    {
        VkCommandBuffer[] vkCommandBuffers = [.. commandBuffers.Select(static item => item.VK().CommandBuffer)];

        fixed (VkCommandBuffer* pCommandBuffers = vkCommandBuffers)
        {
            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
                CommandBufferCount = (uint)commandBuffers.Length,
                PCommandBuffers = pCommandBuffers
            };

            Context.Vk.QueueSubmit(queue, 1, &submitInfo, default).ThrowIfError();
        }
    }

    protected override void DebugName(string name)
    {
        DebugUtilsObjectNameInfoEXT nameInfo = new()
        {
            SType = StructureType.DebugUtilsObjectNameInfoExt,
            ObjectType = ObjectType.Queue,
            ObjectHandle = (ulong)queue.Handle,
            PObjectName = Allocator.AllocUTF8(name)
        };

        Context.ExtDebugUtils!.SetDebugUtilsObjectName(Context.Device, &nameInfo).ThrowIfError();
    }
}
