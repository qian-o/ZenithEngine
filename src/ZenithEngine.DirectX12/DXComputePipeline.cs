using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal class DXComputePipeline : ComputePipeline
{
    public DXComputePipeline(GraphicsContext context,
                             ref readonly ComputePipelineDesc desc) : base(context, in desc)
    {
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
