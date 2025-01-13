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

    public VKShaderTable(GraphicsContext context, VKRayTracingPipeline pipeline) : base(context)
    {
        uint handleSize = 32;
        uint handleSizeAligned = Utils.AlignedSize(handleSize, 64u);

        uint missCount = (uint)pipeline.Desc.Shaders.Miss.Length;
        uint hitGroupCount = (uint)pipeline.Desc.HitGroups.Length;
        uint groupCount = 1 + missCount + hitGroupCount;

        uint missSize = missCount * handleSize;
        uint hitGroupSize = hitGroupCount * handleSize;
        uint groupSize = groupCount * handleSize;

        byte* group = Allocator.Alloc<byte>(groupSize);
        Context.KhrRayTracingPipeline!.GetRayTracingCaptureReplayShaderGroupHandles(Context.Device,
                                                                                    pipeline.Pipeline,
                                                                                    0,
                                                                                    groupCount,
                                                                                    groupSize,
                                                                                    group).ThrowIfError();

        uint missSizeAligned = missCount * handleSizeAligned;
        uint hitGroupSizeAligned = hitGroupCount * handleSizeAligned;

        RayGenBuffer = new(Context, handleSizeAligned, BufferUsageFlags.ShaderBindingTableBitKhr, true);
        MissBuffer = new(Context, missSizeAligned, BufferUsageFlags.ShaderBindingTableBitKhr, true);
        HitGroupBuffer = new(Context, hitGroupSizeAligned, BufferUsageFlags.ShaderBindingTableBitKhr, true);

        CopyHandles(RayGenBuffer, 1);
        group += handleSize;

        CopyHandles(MissBuffer, missCount);
        group += missSize;

        CopyHandles(HitGroupBuffer, hitGroupCount);
        group += hitGroupSize;

        RayGenRegion = new()
        {
            DeviceAddress = RayGenBuffer.Address,
            Stride = handleSizeAligned,
            Size = handleSizeAligned
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

        Allocator.Free(group);

        void CopyHandles(VKBuffer buffer, uint count)
        {
            MappedResource mapped = Context.MapMemory(RayGenBuffer, MapMode.Write);

            for (uint i = 0; i < count; i++)
            {
                Unsafe.CopyBlock((byte*)(mapped.Data + (i * handleSizeAligned)),
                                 group + (i * handleSize),
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
