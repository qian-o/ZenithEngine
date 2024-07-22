using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal sealed unsafe class SharedCommandPool : DeviceResource
{
    private readonly Queue _transferQueue;
    private readonly VkCommandPool _commandPool;
    private readonly Fence _fence;

    public SharedCommandPool(GraphicsDevice graphicsDevice, Queue transferQueue) : base(graphicsDevice)
    {
        CommandPoolCreateInfo createInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = transferQueue.FamilyIndex,
            Flags = CommandPoolCreateFlags.TransientBit | CommandPoolCreateFlags.ResetCommandBufferBit
        };

        VkCommandPool commandPool;
        Vk.CreateCommandPool(Device, &createInfo, null, &commandPool).ThrowCode();

        _transferQueue = transferQueue;
        _commandPool = commandPool;
        _fence = new Fence(graphicsDevice);
    }

    public CommandBuffer BeginNewCommandBuffer()
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

        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        Vk.BeginCommandBuffer(commandBuffer, &beginInfo).ThrowCode();

        return commandBuffer;
    }

    public void EndAndSubmitCommandBuffer(CommandBuffer commandBuffer)
    {
        Vk.EndCommandBuffer(commandBuffer).ThrowCode();

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer
        };

        Vk.QueueSubmit(_transferQueue.Handle, 1, &submitInfo, _fence.Handle).ThrowCode();

        _fence.WaitAndReset();
    }

    protected override void Destroy()
    {
        _fence.Dispose();

        Vk.DestroyCommandPool(Device, _commandPool, null);
    }
}
