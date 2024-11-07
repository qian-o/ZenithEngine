using System.Numerics;

namespace Graphics.Engine.Descriptions;

public struct RenderStateDescription
{
    /// <summary>
    /// Gets or sets the Rasterizer State.
    /// </summary>
    public RasterizerStateDescription RasterizerState { get; set; }

    /// <summary>
    /// Gets or sets the DepthStencil state.
    /// </summary>
    public DepthStencilStateDescription DepthStencilState { get; set; }

    /// <summary>
    /// Gets or sets the BlendState state.
    /// </summary>
    public BlendStateDescription BlendState { get; set; }

    /// <summary>
    /// The reference value to use when doing a stencil test.
    /// </summary>
    public int StencilReference { get; set; }

    /// <summary>
    /// Array of blend factors, one for each RGBA component.
    /// </summary>
    public Vector4? BlendFactor { get; set; }
}
