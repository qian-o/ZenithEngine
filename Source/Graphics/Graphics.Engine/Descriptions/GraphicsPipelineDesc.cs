namespace Graphics.Engine.Descriptions;

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
}
