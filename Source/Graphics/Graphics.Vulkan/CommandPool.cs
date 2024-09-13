using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public sealed unsafe class CommandPool : VulkanObject<VkCommandPool>
{
    public CommandPool(VulkanResources vkRes, uint queueFamilyIndex) : base(vkRes, ObjectType.CommandPool)
    {
        CommandPoolCreateInfo createInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = queueFamilyIndex,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };

        VkCommandPool commandPool;
        VkRes.Vk.CreateCommandPool(VkRes.GraphicsDevice.Handle, &createInfo, null, &commandPool).ThrowCode();

        Handle = commandPool;
    }

    internal override VkCommandPool Handle { get; }

    public CommandBuffer AllocateCommandBuffer()
    {
        CommandBufferAllocateInfo allocateInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = Handle,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        CommandBuffer commandBuffer;
        VkRes.Vk.AllocateCommandBuffers(VkRes.GraphicsDevice.Handle, &allocateInfo, &commandBuffer).ThrowCode();

        return commandBuffer;
    }

    public void FreeCommandBuffer(CommandBuffer commandBuffer)
    {
        VkRes.Vk.FreeCommandBuffers(VkRes.GraphicsDevice.Handle, Handle, 1, &commandBuffer);
    }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    protected override void Destroy()
    {
        VkRes.Vk.DestroyCommandPool(VkRes.GraphicsDevice.Handle, Handle, null);
    }
}
