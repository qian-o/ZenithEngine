using Silk.NET.Vulkan;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKDeviceMemory : GraphicsResource
{
    public VkDeviceMemory DeviceMemory;

    public VKDeviceMemory(GraphicsContext context,
                          bool isDynamic,
                          MemoryRequirements requirements,
                          bool dedicated,
                          VkImage? dedicatedImage,
                          VkBuffer? dedicatedBuffer) : base(context)
    {
        MemoryPropertyFlags flags = isDynamic
            ? MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit
            : MemoryPropertyFlags.DeviceLocalBit;

        MemoryAllocateInfo allocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = Context.FindMemoryTypeIndex(requirements.MemoryTypeBits, flags)
        };

        allocateInfo.AddNext(out MemoryAllocateFlagsInfo flagsInfo);

        flagsInfo.Flags = MemoryAllocateFlags.AddressBit;

        if (dedicated)
        {
            allocateInfo.AddNext(out MemoryDedicatedAllocateInfo dedicatedInfo);

            dedicatedInfo.Image = dedicatedImage ?? default;
            dedicatedInfo.Buffer = dedicatedBuffer ?? default;
        }

        Context.Vk.AllocateMemory(Context.Device,
                                  &allocateInfo,
                                  null,
                                  out DeviceMemory).ThrowIfError();
    }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
        Context.Vk.FreeMemory(Context.Device, DeviceMemory, null);
    }
}
