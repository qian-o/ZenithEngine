using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class GraphicsPipeline(GraphicsContext context,
                                       ref readonly GraphicsPipelineDesc desc) : Pipeline(context)
{
    public GraphicsPipelineDesc Desc { get; } = desc;
}
