using Silk.NET.Vulkan;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using CommandBuffer = ZenithEngine.Common.Graphics.CommandBuffer;

namespace ZenithEngine.Vulkan;

internal class VKCommandProcessor : CommandProcessor
{
    private readonly VkQueue queue;

    public VKCommandProcessor(GraphicsContext context,
                              CommandProcessorType type) : base(context, type)
    {
        uint queueFamilyIndex = type switch
        {
            CommandProcessorType.Direct => ((VKGraphicsContext)context).DirectQueueFamilyIndex,
            CommandProcessorType.Copy => ((VKGraphicsContext)context).CopyQueueFamilyIndex,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        queue = Context.Vk.GetDeviceQueue(Context.Device, queueFamilyIndex, 0);
    }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public override void WaitIdle()
    {
        Context.Vk.QueueWaitIdle(queue).ThrowIfError();
    }

    protected override CommandBuffer CreateCommandBuffer()
    {
        throw new NotImplementedException();
    }

    protected override void SubmitCommandBuffer(CommandBuffer commandBuffer)
    {
        throw new NotImplementedException();
    }

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.Queue, (ulong)queue.Handle, name);
    }
}
