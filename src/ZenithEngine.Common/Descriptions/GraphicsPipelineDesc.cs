using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct GraphicsPipelineDesc
{
    /// <summary>
    /// The render state description.
    /// </summary>
    public RenderStateDesc RenderStates;

    /// <summary>
    /// The shader state description.
    /// </summary>
    public GraphicsShaderDesc Shaders;

    /// <summary>
    /// vertex input layout.
    /// </summary>
    public LayoutDesc[] InputLayouts;

    /// <summary>
    /// Describes the resource layouts input array.
    /// </summary>
    public ResourceLayout[] ResourceLayouts;

    /// <summary>
    /// Define how vertices are interpreted and rendered by the pipeline.
    /// </summary>
    public PrimitiveTopology PrimitiveTopology;

    /// <summary>
    /// A description of the output attachments of the pipeline.
    /// </summary>
    public OutputDesc Outputs;

    public static GraphicsPipelineDesc New(GraphicsShaderDesc shaders,
                                           LayoutDesc[] inputLayouts,
                                           ResourceLayout[] resourceLayouts,
                                           OutputDesc outputs,
                                           RenderStateDesc? renderStates = null,
                                           PrimitiveTopology primitiveTopology = PrimitiveTopology.TriangleList)
    {
        renderStates ??= new();

        return new()
        {
            RenderStates = renderStates.Value,
            Shaders = shaders,
            InputLayouts = inputLayouts,
            ResourceLayouts = resourceLayouts,
            PrimitiveTopology = primitiveTopology,
            Outputs = outputs
        };
    }
}
