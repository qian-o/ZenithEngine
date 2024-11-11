using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKCommandBuffer : CommandBuffer
{
    public VKCommandBuffer(Context context, VKCommandProcessor processor) : base(context)
    {
        Processor = processor;

        CommandPoolCreateInfo createInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = Processor.FamilyIndex,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };

        VkCommandPool commandPool;
        Context.Vk.CreateCommandPool(Context.Device, &createInfo, null, &commandPool).ThrowCode();

        CommandBufferAllocateInfo allocateInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        VkCommandBuffer commandBuffer;
        Context.Vk.AllocateCommandBuffers(Context.Device, &allocateInfo, &commandBuffer).ThrowCode();

        CommandPool = commandPool;
        CommandBuffer = commandBuffer;
    }

    public new VKContext Context => (VKContext)base.Context;

    public VKCommandProcessor Processor { get; }

    public VkCommandPool CommandPool { get; }

    public VkCommandBuffer CommandBuffer { get; }

    public override void Begin()
    {
    }

    public override void Reset()
    {
    }

    public override void Commit()
    {
    }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.CommandPool, CommandPool.Handle, name);
        Context.SetDebugName(ObjectType.CommandBuffer, (ulong)CommandBuffer.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyCommandPool(Context.Device, CommandPool, null);
    }

    protected override void ClearCache()
    {
    }

    protected override void EndInternal()
    {
    }
}
