using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class RayTracingPipeline(GraphicsContext context,
                                         ref readonly RayTracingPipelineDesc desc) : Pipeline(context)
{
    private RayTracingPipelineDesc descInternal = desc;

    public ref RayTracingPipelineDesc Desc => ref descInternal;
}
