using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal class DXGraphicsPipeline : GraphicsPipeline
{
    public DXGraphicsPipeline(GraphicsContext context,
                              ref readonly GraphicsPipelineDesc desc) : base(context, in desc)
    {

    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
