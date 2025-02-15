using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXBottomLevelAS : BottomLevelAS
{
    public DXBottomLevelAS(GraphicsContext context,
                           ComPtr<ID3D12GraphicsCommandList4> commandList,
                           ref readonly BottomLevelASDesc desc) : base(context, in desc)
    {
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
