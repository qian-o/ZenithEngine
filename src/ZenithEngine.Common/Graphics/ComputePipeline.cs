using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class ComputePipeline(GraphicsContext context,
                                      ref readonly ComputePipelineDesc desc) : Pipeline(context)
{
    private ComputePipelineDesc descInternal = desc;

    public ref ComputePipelineDesc Desc => ref descInternal;
}
