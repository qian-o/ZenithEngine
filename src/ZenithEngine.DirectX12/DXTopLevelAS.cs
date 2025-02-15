using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXTopLevelAS : TopLevelAS
{
    public DXTopLevelAS(GraphicsContext context,
                        ComPtr<ID3D12GraphicsCommandList4> commandList,
                        ref readonly TopLevelASDesc desc) : base(context, in desc)
    {
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}