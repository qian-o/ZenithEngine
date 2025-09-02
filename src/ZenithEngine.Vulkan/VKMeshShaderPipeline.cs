using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKMeshShaderPipeline : MeshShaderPipeline
{
    public VKMeshShaderPipeline(GraphicsContext context,
                                ref readonly MeshShaderPipelineDesc desc) : base(context, in desc)
    {
    }

    protected override void SetName(string name)
    {
        throw new NotImplementedException();
    }

    protected override void Destroy()
    {
        throw new NotImplementedException();
    }
}
