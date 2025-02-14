using Silk.NET.Vulkan;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKFence : GraphicsResource
{
    public VkFence Fence;

    public VKFence(GraphicsContext context) : base(context)
    {
        FenceCreateInfo createInfo = new()
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit
        };

        Context.Vk.CreateFence(Context.Device,
                               in createInfo,
                               null,
                               out Fence).ThrowIfError();

        Context.Vk.ResetFences(Context.Device,
                               1,
                               in Fence).ThrowIfError();
    }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public void Wait()
    {
        Context.Vk.WaitForFences(Context.Device,
                                 1,
                                 in Fence,
                                 true,
                                 ulong.MaxValue).ThrowIfError();

        Context.Vk.ResetFences(Context.Device,
                               1,
                               in Fence).ThrowIfError();
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyFence(Context.Device, Fence, null);
    }
}
