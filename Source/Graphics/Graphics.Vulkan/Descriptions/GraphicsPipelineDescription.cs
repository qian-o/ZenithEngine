using Graphics.Core;

namespace Graphics.Vulkan;

public readonly record struct GraphicsPipelineDescription
{
    public GraphicsPipelineDescription(BlendStateDescription blendState,
                                       DepthStencilStateDescription depthStencilState,
                                       RasterizerStateDescription rasterizerState,
                                       PrimitiveTopology primitiveTopology,
                                       ShaderSetDescription shaderSet,
                                       ResourceLayout[] resourceLayouts,
                                       OutputDescription outputs)
    {
        BlendState = blendState;
        DepthStencilState = depthStencilState;
        RasterizerState = rasterizerState;
        PrimitiveTopology = primitiveTopology;
        ShaderSet = shaderSet;
        ResourceLayouts = resourceLayouts;
        Outputs = outputs;
    }

    /// <summary>
    /// A description of the blend state, which controls how color values are blended into each color target.
    /// </summary>
    public BlendStateDescription BlendState { get; init; }

    /// <summary>
    /// A description of the depth stencil state, which controls depth tests, writing, and comparisons.
    /// </summary>
    public DepthStencilStateDescription DepthStencilState { get; init; }

    /// <summary>
    /// A description of the rasterizer state, which controls culling, clipping, scissor, and polygon-fill behavior.
    /// </summary>
    public RasterizerStateDescription RasterizerState { get; init; }

    /// <summary>
    /// This controls the primitive topology type.
    /// </summary>
    public PrimitiveTopology PrimitiveTopology { get; init; }

    /// <summary>
    /// A description of the shader set to be used.
    /// </summary>
    public ShaderSetDescription ShaderSet { get; init; }

    /// <summary>
    /// This controls the resource layout of the shaders.
    /// </summary>
    public ResourceLayout[] ResourceLayouts { get; init; }

    /// <summary>
    /// Describes the color and depth stencil outputs.
    /// </summary>
    public OutputDescription Outputs { get; init; }
}
