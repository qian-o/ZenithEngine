using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXShaderTable : GraphicsResource
{
    public DXShaderTable(GraphicsContext context,
                         ComPtr<ID3D12StateObject> stateObject,
                         string[] rayGenExports,
                         string[] missExports,
                         string[] hitGroupExports) : base(context)
    {
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
                           ResourceFlags.None,
                           new(HeapType.Upload),
                           ResourceStates.GenericRead);
        MissBuffer = new(Context,
                         in missDesc,
                         ResourceFlags.None,
                         new(HeapType.Upload),
                         ResourceStates.GenericRead);
        HitGroupBuffer = new(Context,
                             in hitGroupDesc,
                             ResourceFlags.None,
                             new(HeapType.Upload),
                             ResourceStates.GenericRead);
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
    }
}
