using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal sealed unsafe class Semaphore : VulkanObject<VkSemaphore>
{
    public Semaphore(VulkanResources vkRes) : base(vkRes, ObjectType.Semaphore)
    {
        SemaphoreCreateInfo createInfo = new()
        {
            SType = StructureType.SemaphoreCreateInfo
        };

        VkSemaphore semaphore;
        VkRes.Vk.CreateSemaphore(VkRes.GetDevice(), &createInfo, null, &semaphore).ThrowCode();

        Handle = semaphore;
    }

    internal override VkSemaphore Handle { get; }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    protected override void Destroy()
    {
        VkRes.Vk.DestroySemaphore(VkRes.GetDevice(), Handle, null);
    }
}
