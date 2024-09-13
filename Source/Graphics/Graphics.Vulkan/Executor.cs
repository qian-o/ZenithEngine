using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Executor : VulkanObject<VkQueue>
{
    public Executor(VulkanResources vkRes, uint queueFamilyIndex) : base(vkRes, ObjectType.Queue)
    {
        VkQueue queue;
        VkRes.Vk.GetDeviceQueue(VkRes.GetDevice(), queueFamilyIndex, 0, &queue);

        Handle = queue;
        FamilyIndex = queueFamilyIndex;
    }
    internal override VkQueue Handle { get; }

    internal uint FamilyIndex { get; }

    internal override ulong[] GetHandles()
    {
        return [(ulong)Handle.Handle];
    }

    protected override void Destroy()
    {
    }
}
