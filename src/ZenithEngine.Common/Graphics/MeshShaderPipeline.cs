using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class MeshShaderPipeline(GraphicsContext context,
                                         ref readonly MeshShaderPipelineDesc desc) : Pipeline(context)
{
    private MeshShaderPipelineDesc descInternal = desc;

    public ref MeshShaderPipelineDesc Desc => ref descInternal;
}
