using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal sealed unsafe class CommandPool : DeviceResource
{
    public CommandPool(GraphicsDevice graphicsDevice, uint queueFamilyIndex) : base(graphicsDevice)
    {
        CommandPoolCreateInfo createInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = queueFamilyIndex,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };

        VkCommandPool commandPool;
        Vk.CreateCommandPool(Device, &createInfo, null, &commandPool).ThrowCode();

        Handle = commandPool;
    }

    public VkCommandPool Handle { get; }

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
        Vk.AllocateCommandBuffers(Device, &allocateInfo, &commandBuffer).ThrowCode();

        return commandBuffer;
    }

    public void FreeCommandBuffer(CommandBuffer commandBuffer)
    {
        Vk.FreeCommandBuffers(Device, Handle, 1, &commandBuffer);
    }

    protected override void Destroy()
    {
        Vk.DestroyCommandPool(Device, Handle, null);
    }
}
