using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class StagingCommandPool : VulkanObject<VkCommandPool>
{
    public StagingCommandPool(VulkanResources vkRes, Executor taskExecutor) : base(vkRes, ObjectType.CommandPool)
    {
        CommandPoolCreateInfo createInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = taskExecutor.FamilyIndex,
            Flags = CommandPoolCreateFlags.TransientBit
        };

        VkCommandPool commandPool;
        VkRes.Vk.CreateCommandPool(VkRes.GetDevice(), &createInfo, null, &commandPool).ThrowCode();

        Handle = commandPool;
        TaskExecutor = taskExecutor;
        Fence = new Fence(VkRes);
    }

    internal override VkCommandPool Handle { get; }

    internal Executor TaskExecutor { get; }

    internal Fence Fence { get; }

    public CommandBuffer BeginNewCommandBuffer()
    {
        CommandBufferAllocateInfo allocateInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = Handle,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        CommandBuffer commandBuffer;
        VkRes.Vk.AllocateCommandBuffers(VkRes.GetDevice(), &allocateInfo, &commandBuffer).ThrowCode();

        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        VkRes.Vk.BeginCommandBuffer(commandBuffer, &beginInfo).ThrowCode();

        return commandBuffer;
    }

    public void EndAndSubmitCommandBuffer(CommandBuffer commandBuffer)
    {
        VkRes.Vk.EndCommandBuffer(commandBuffer).ThrowCode();

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer
        };

        VkRes.Vk.QueueSubmit(TaskExecutor.Handle, 1, &submitInfo, Fence.Handle).ThrowCode();

        Fence.WaitAndReset();

        VkRes.Vk.FreeCommandBuffers(VkRes.GetDevice(), Handle, 1, &commandBuffer);
    }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    protected override void Destroy()
    {
        Fence.Dispose();

        VkRes.Vk.DestroyCommandPool(VkRes.GetDevice(), Handle, null);
    }
}
