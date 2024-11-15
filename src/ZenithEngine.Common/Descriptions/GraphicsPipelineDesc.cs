using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct GraphicsPipelineDesc
{
    /// <summary>
    /// The render state description.
    /// </summary>
    public RenderStateDesc RenderStates { get; set; }

    /// <summary>
    /// The shader state description.
    /// </summary>
    public GraphicsShaderDesc Shaders { get; set; }

    /// <summary>
    /// vertex input layout.
    /// </summary>
    public LayoutDesc[] InputLayouts { get; set; }

    /// <summary>
    /// Describes the resource layouts input array.
    /// </summary>
    public ResourceLayout[] ResourceLayouts { get; set; }

    /// <summary>
    /// Define how vertices are interpreted and rendered by the pipeline.
    /// </summary>
    public PrimitiveTopology PrimitiveTopology { get; set; }

    /// <summary>
    /// A description of the output attachments of the pipeline.
    /// </summary>
    public OutputDesc Outputs { get; set; }

    public static GraphicsPipelineDesc Default(GraphicsShaderDesc shaders,
                                               LayoutDesc[] inputLayouts,
                                               OutputDesc outputs,
                                               RenderStateDesc? renderStates = null,
                                               ResourceLayout[]? resourceLayouts = null,
                                               PrimitiveTopology primitiveTopology = PrimitiveTopology.TriangleList)
    {
        renderStates ??= RenderStateDesc.Default();
        resourceLayouts ??= [];

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
