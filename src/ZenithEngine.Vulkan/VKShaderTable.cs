using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;
using ZenithEngine.Common;
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
        uint handleSize = 32;
        uint handleSizeAligned = Utils.AlignedSize(handleSize, 64u);

        uint groupCount = rayGenCount + missCount + hitGroupCount;

        uint rayGenSize = rayGenCount * handleSize;
        uint missSize = missCount * handleSize;
        uint hitGroupSize = hitGroupCount * handleSize;

        uint rayGenSizeAligned = rayGenCount * handleSizeAligned;
        uint missSizeAligned = missCount * handleSizeAligned;
        uint hitGroupSizeAligned = hitGroupCount * handleSizeAligned;

        uint dataSize = rayGenSize + missSize + hitGroupSize;
        byte* data = Allocator.Alloc<byte>(dataSize);
        Context.KhrRayTracingPipeline!.GetRayTracingCaptureReplayShaderGroupHandles(Context.Device,
                                                                                    pipeline,
                                                                                    0,
                                                                                    groupCount,
                                                                                    dataSize,
                                                                                    data).ThrowIfError();


        RayGenBuffer = new(Context, rayGenSizeAligned, BufferUsageFlags.ShaderBindingTableBitKhr, true);
        MissBuffer = new(Context, missSizeAligned, BufferUsageFlags.ShaderBindingTableBitKhr, true);
        HitGroupBuffer = new(Context, hitGroupSizeAligned, BufferUsageFlags.ShaderBindingTableBitKhr, true);

        CopyHandles(RayGenBuffer, rayGenCount);
        data += rayGenSize;

        CopyHandles(MissBuffer, missCount);
        data += missSize;

        CopyHandles(HitGroupBuffer, hitGroupCount);
        data += hitGroupSize;

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

        Allocator.Free(data);

        void CopyHandles(VKBuffer buffer, uint count)
        {
            MappedResource mapped = Context.MapMemory(RayGenBuffer, MapMode.Write);

            for (uint i = 0; i < count; i++)
            {
                Unsafe.CopyBlock((byte*)(mapped.Data + (i * handleSizeAligned)),
                                 data + (i * handleSize),
                                 handleSize);
            }

            Context.UnmapMemory(buffer);
        }
    }

    public VKBuffer RayGenBuffer { get; }

    public VKBuffer MissBuffer { get; }

    public VKBuffer HitGroupBuffer { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        RayGenBuffer.Dispose();
        MissBuffer.Dispose();
        HitGroupBuffer.Dispose();
    }
}
