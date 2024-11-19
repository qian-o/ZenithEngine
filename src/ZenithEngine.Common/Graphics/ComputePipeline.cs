using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class ComputePipeline(GraphicsContext context,
                                      ref readonly ComputePipelineDesc desc) : Pipeline(context)
{
    public ComputePipelineDesc Desc { get; } = desc;
}
