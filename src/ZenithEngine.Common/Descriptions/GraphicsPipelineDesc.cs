using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct GraphicsPipelineDesc(GraphicsShaderDesc shaders,
                                   LayoutDesc[] inputLayouts,
                                   ResourceLayout[] resourceLayouts,
                                   OutputDesc outputs,
                                   RenderStateDesc? renderStates = null,
                                   PrimitiveTopology primitiveTopology = PrimitiveTopology.TriangleList)
{
    /// <summary>
    /// The render state description.
    /// </summary>
    public RenderStateDesc RenderStates = renderStates ?? new();

    /// <summary>
    /// The shader state description.
    /// </summary>
    public GraphicsShaderDesc Shaders = shaders;

    /// <summary>
    /// vertex input layout.
    /// </summary>
    public LayoutDesc[] InputLayouts = inputLayouts;

    /// <summary>
    /// Describes the resource layouts input array.
    /// </summary>
    public ResourceLayout[] ResourceLayouts = resourceLayouts;

    /// <summary>
    /// Define how vertices are interpreted and rendered by the pipeline.
    /// </summary>
    public PrimitiveTopology PrimitiveTopology = primitiveTopology;

    /// <summary>
    /// A description of the output attachments of the pipeline.
    /// </summary>
    public OutputDesc Outputs = outputs;
}
