using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXQueryHeap : QueryHeap
{
    public ComPtr<ID3D12QueryHeap> QueryHeap;

    public DXQueryHeap(GraphicsContext context,
                       ref readonly QueryHeapDesc desc) : base(context, in desc)
    {
        DxQueryHeapDesc queryHeapDesc = new()
        {
            Type = DXFormats.GetQueryHeapType(desc.Type),
            Count = desc.Count
        };

        Context.Device.CreateQueryHeap(&queryHeapDesc, out QueryHeap).ThrowIfError();

        BufferDesc backBufferDesc = new(desc.Count * sizeof(ulong));

        BackBuffer = new DXBuffer(Context,
                                  in backBufferDesc,
                                  HeapType.Readback,
                                  ResourceFlags.None,
                                  ResourceStates.CopyDest);
    }

    public DXBuffer BackBuffer { get; }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public override void GetData(int startIndex, Span<ulong> data)
    {
        MappedResource resource = Context.MapMemory(BackBuffer, MapMode.Read);

        Span<ulong> bufferData = new((void*)(resource.Data + (startIndex * sizeof(ulong))), data.Length);
        bufferData.CopyTo(data);

        Context.UnmapMemory(BackBuffer);
    }

    protected override void SetName(string name)
    {
        QueryHeap.SetName(name);
    }

    protected override void Destroy()
    {
        BackBuffer.Dispose();
        QueryHeap.Dispose();
    }
}
