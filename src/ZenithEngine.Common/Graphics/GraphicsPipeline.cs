using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class GraphicsPipeline(GraphicsContext context,
                                       ref readonly GraphicsPipelineDesc desc) : Pipeline(context)
{
    private GraphicsPipelineDesc descInternal = desc;

    public ref GraphicsPipelineDesc Desc => ref descInternal;
}
