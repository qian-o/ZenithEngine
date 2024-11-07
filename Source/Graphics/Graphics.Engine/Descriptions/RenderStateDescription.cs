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

    /// <summary>
    /// 32-bit sample coverage. The default value is 0xFFFFFF. See remarks.
    /// </summary>
    public int? SampleMask { get; set; }

    public static RenderStateDescription Default()
    {
        return new()
        {
            RasterizerState = RasterizerStateDescription.Default(),
            DepthStencilState = DepthStencilStateDescription.Default(),
            BlendState = BlendStateDescription.Default(),
            StencilReference = 0,
            BlendFactor = null,
            SampleMask = 16777215
        };
    }
}
