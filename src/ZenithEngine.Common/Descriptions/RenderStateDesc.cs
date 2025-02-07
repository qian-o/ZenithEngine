using Silk.NET.Maths;

namespace ZenithEngine.Common.Descriptions;

public struct RenderStateDesc(RasterizerStateDesc? rasterizerState = null,
                              DepthStencilStateDesc? depthStencilState = null,
                              BlendStateDesc? blendState = null,
                              int stencilReference = 0,
                              Vector4D<float>? blendFactor = null)
{
    public RenderStateDesc()
    {
    }

    /// <summary>
    /// Gets or sets the Rasterizer State.
    /// </summary>
    public RasterizerStateDesc RasterizerState = rasterizerState ?? new();

    /// <summary>
    /// Gets or sets the DepthStencil state.
    /// </summary>
    public DepthStencilStateDesc DepthStencilState = depthStencilState ?? new();

    /// <summary>
    /// Gets or sets the BlendState state.
    /// </summary>
    public BlendStateDesc BlendState = blendState ?? new();

    /// <summary>
    /// The reference value to use when doing a stencil test.
    /// </summary>
    public int StencilReference = stencilReference;

    /// <summary>
    /// Array of blend factors, one for each RGBA component.
    /// </summary>
    public Vector4D<float>? BlendFactor = blendFactor;
}