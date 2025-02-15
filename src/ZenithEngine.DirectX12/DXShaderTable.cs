using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXShaderTable : GraphicsResource
{
    public GpuVirtualAddressRange RayGenRange;
    public GpuVirtualAddressRangeAndStride MissRange;
    public GpuVirtualAddressRangeAndStride HitGroupRange;

    public DXShaderTable(GraphicsContext context,
                         ComPtr<ID3D12StateObject> stateObject,
                         string[] rayGenExports,
                         string[] missExports,
                         string[] hitGroupExports) : base(context)
    {
        stateObject.QueryInterface(out ComPtr<ID3D12StateObjectProperties> stateObjectProperties).ThrowIfError();

        const uint handleSize = 32;

        uint handleSizeAligned = Utils.AlignedSize<uint>(handleSize, 64);

        uint rayGenCount = (uint)rayGenExports.Length;
        uint missCount = (uint)missExports.Length;
        uint hitGroupCount = (uint)hitGroupExports.Length;

        uint rayGenSizeAligned = rayGenCount * handleSizeAligned;
        uint missSizeAligned = missCount * handleSizeAligned;
        uint hitGroupSizeAligned = hitGroupCount * handleSizeAligned;

        BufferDesc rayGenDesc = new(rayGenSizeAligned);
        BufferDesc missDesc = new(missSizeAligned);
        BufferDesc hitGroupDesc = new(hitGroupSizeAligned);

        RayGenBuffer = new(Context,
                           in rayGenDesc,
                           HeapType.Upload,
                           ResourceFlags.None,
                           ResourceStates.GenericRead);

        MissBuffer = new(Context,
                         in missDesc,
                         HeapType.Upload,
                         ResourceFlags.None,
                         ResourceStates.GenericRead);

        HitGroupBuffer = new(Context,
                             in hitGroupDesc,
                             HeapType.Upload,
                             ResourceFlags.None,
                             ResourceStates.GenericRead);

        CopyHandles(RayGenBuffer, rayGenExports);
        CopyHandles(MissBuffer, missExports);
        CopyHandles(HitGroupBuffer, hitGroupExports);

        RayGenRange = new()
        {
            StartAddress = RayGenBuffer.Resource.GetGPUVirtualAddress(),
            SizeInBytes = rayGenSizeAligned
        };

        MissRange = new()
        {
            StartAddress = MissBuffer.Resource.GetGPUVirtualAddress(),
            SizeInBytes = missSizeAligned,
            StrideInBytes = handleSizeAligned
        };

        HitGroupRange = new()
        {
            StartAddress = HitGroupBuffer.Resource.GetGPUVirtualAddress(),
            SizeInBytes = hitGroupSizeAligned,
            StrideInBytes = handleSizeAligned
        };

        Allocator.Release();

        stateObjectProperties.Dispose();

        void CopyHandles(DXBuffer buffer, string[] exports)
        {
            MappedResource mapped = Context.MapMemory(buffer, MapMode.Write);

            for (int i = 0; i < exports.Length; i++)
            {
                Unsafe.CopyBlock((byte*)(mapped.Data + (i * handleSizeAligned)),
                                 stateObjectProperties.GetShaderIdentifier((char*)Allocator.AllocUni(exports[i])),
                                 handleSize);
            }

            Context.UnmapMemory(buffer);
        }
    }

    public DXBuffer RayGenBuffer { get; }

    public DXBuffer MissBuffer { get; }

    public DXBuffer HitGroupBuffer { get; }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

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
