using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKShaderTable : GraphicsResource
{
    public StridedDeviceAddressRegionKHR RayGenRegion;
    public StridedDeviceAddressRegionKHR MissRegion;
    public StridedDeviceAddressRegionKHR HitGroupRegion;

    public VKShaderTable(GraphicsContext context,
                         VkPipeline pipeline,
                         uint rayGenCount,
                         uint missCount,
                         uint hitGroupCount) : base(context)
    {
        const uint handleSize = 32u;

        uint handleSizeAligned = Utils.Align(handleSize, 64u);

        uint rayGenSize = rayGenCount * handleSize;
        uint missSize = missCount * handleSize;
        uint hitGroupSize = hitGroupCount * handleSize;

        uint rayGenSizeAligned = rayGenCount * handleSizeAligned;
        uint missSizeAligned = missCount * handleSizeAligned;
        uint hitGroupSizeAligned = hitGroupCount * handleSizeAligned;

        uint groupCount = rayGenCount + missCount + hitGroupCount;
        uint dataSize = rayGenSize + missSize + hitGroupSize;
        byte* data = Allocator.Alloc<byte>(dataSize);

        Context.KhrRayTracingPipeline!.GetRayTracingShaderGroupHandles(Context.Device,
                                                                       pipeline,
                                                                       0,
                                                                       groupCount,
                                                                       dataSize,
                                                                       data).ThrowIfError();

        BufferDesc rayGenDesc = new(rayGenSizeAligned);
        BufferDesc missDesc = new(missSizeAligned);
        BufferDesc hitGroupDesc = new(hitGroupSizeAligned);

        RayGenBuffer = new(Context,
                           in rayGenDesc,
                           true,
                           BufferUsageFlags.ShaderBindingTableBitKhr);

        MissBuffer = new(Context,
                         in missDesc,
                         true,
                         BufferUsageFlags.ShaderBindingTableBitKhr);

        HitGroupBuffer = new(Context,
                             in hitGroupDesc,
                             true,
                             BufferUsageFlags.ShaderBindingTableBitKhr);

        CopyHandles(RayGenBuffer, rayGenCount);
        CopyHandles(MissBuffer, missCount);
        CopyHandles(HitGroupBuffer, hitGroupCount);

        RayGenRegion = new()
        {
            DeviceAddress = RayGenBuffer.Address,
            Stride = handleSizeAligned,
            Size = rayGenSizeAligned
        };

        MissRegion = new()
        {
            DeviceAddress = MissBuffer.Address,
            Stride = handleSizeAligned,
            Size = missSizeAligned
        };

        HitGroupRegion = new()
        {
            DeviceAddress = HitGroupBuffer.Address,
            Stride = handleSizeAligned,
            Size = hitGroupSizeAligned
        };

        Allocator.Release();

        void CopyHandles(VKBuffer buffer, uint count)
        {
            MappedResource mapped = Context.MapMemory(buffer, MapMode.Write);

            for (uint i = 0; i < count; i++)
            {
                Unsafe.CopyBlock((byte*)(mapped.Data + (i * handleSizeAligned)),
                                 data,
                                 handleSize);

                data += handleSize;
            }

            Context.UnmapMemory(buffer);
        }
    }

    public VKBuffer RayGenBuffer { get; }

    public VKBuffer MissBuffer { get; }

    public VKBuffer HitGroupBuffer { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
        RayGenBuffer.Dispose();
        MissBuffer.Dispose();
        HitGroupBuffer.Dispose();
    }
}
