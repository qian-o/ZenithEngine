using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct MeshShaderPipelineDesc(MeshShaderDesc shaders,
                                     ResourceLayout[] resourceLayouts,
                                     OutputDesc outputs,
                                     RenderStateDesc? renderStates = null)
{
    public MeshShaderPipelineDesc() : this(new(), [], new(), null)
    {
    }

    /// <summary>
    /// The render state description.
    /// </summary>
    public RenderStateDesc RenderStates = renderStates ?? new();

    /// <summary>
    /// The shader state description.
    /// </summary>
    public MeshShaderDesc Shaders = shaders;

    /// <summary>
    /// Describes the resource layouts input array.
    /// </summary>
    public ResourceLayout[] ResourceLayouts = resourceLayouts;

    /// <summary>
    /// A description of the output attachments of the pipeline.
    /// </summary>
    public OutputDesc Outputs = outputs;
}
