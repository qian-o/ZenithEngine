using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Fence : VulkanObject<VkFence>
{
    private readonly VkFence _fence;

    internal Fence(VulkanResources vkRes) : base(vkRes, ObjectType.Fence)
    {
        FenceCreateInfo createInfo = new()
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit
        };

        VkFence fence;
        VkRes.Vk.CreateFence(VkRes.GetDevice(), &createInfo, null, &fence).ThrowCode();
        VkRes.Vk.ResetFences(VkRes.GetDevice(), 1, &fence).ThrowCode();

        _fence = fence;
    }

    internal override VkFence Handle { get; }

    public void WaitAndReset()
    {
        fixed (VkFence* fence = &_fence)
        {
            VkRes.Vk.WaitForFences(VkRes.GetDevice(), 1, fence, Vk.True, ulong.MaxValue).ThrowCode();
            VkRes.Vk.ResetFences(VkRes.GetDevice(), 1, fence).ThrowCode();
        }
    }

    internal override ulong[] GetHandles()
    {
        return [_fence.Handle];
    }

    protected override void Destroy()
    {
        VkRes.Vk.DestroyFence(VkRes.GetDevice(), _fence, null);
    }
}
