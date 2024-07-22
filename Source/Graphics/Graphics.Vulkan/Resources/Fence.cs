using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal sealed unsafe class Fence : DeviceResource
{
    private readonly VkFence _fence;

    public Fence(GraphicsDevice graphicsDevice) : base(graphicsDevice)
    {
        FenceCreateInfo createInfo = new()
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit
        };

        VkFence fence;
        Vk.CreateFence(Device, &createInfo, null, &fence).ThrowCode();
        Vk.ResetFences(Device, 1, &fence).ThrowCode();

        _fence = fence;
    }

    public VkFence Handle => _fence;

    public void Wait()
    {
        fixed (VkFence* fence = &_fence)
        {
            Vk.WaitForFences(Device, 1, fence, Vk.True, ulong.MaxValue).ThrowCode();
        }
    }

    public void Reset()
    {
        fixed (VkFence* fence = &_fence)
        {
            Vk.ResetFences(Device, 1, fence).ThrowCode();
        }
    }

    public void WaitAndReset()
    {
        Wait();
        Reset();
    }

    protected override void Destroy()
    {
        Vk.DestroyFence(Device, _fence, null);
    }
}
