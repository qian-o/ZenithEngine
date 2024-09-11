using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal sealed unsafe class CommandPool : DeviceResource
{
    private readonly VkCommandPool _commandPool;

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

        _commandPool = commandPool;
    }

    public VkCommandPool Handle => _commandPool;

    public CommandBuffer AllocateCommandBuffer()
    {
        CommandBufferAllocateInfo allocateInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        CommandBuffer commandBuffer;
        Vk.AllocateCommandBuffers(Device, &allocateInfo, &commandBuffer).ThrowCode();

        return commandBuffer;
    }

    public void FreeCommandBuffer(CommandBuffer commandBuffer)
    {
        Vk.FreeCommandBuffers(Device, _commandPool, 1, &commandBuffer);
    }

    protected override void Destroy()
    {
        Vk.DestroyCommandPool(Device, _commandPool, null);
    }
}
