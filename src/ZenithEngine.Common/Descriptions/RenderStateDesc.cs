using Silk.NET.Maths;

namespace ZenithEngine.Common.Descriptions;

public struct RenderStateDesc
{
    /// <summary>
    /// Gets or sets the Rasterizer State.
    /// </summary>
    public RasterizerStateDesc RasterizerState;

    /// <summary>
    /// Gets or sets the DepthStencil state.
    /// </summary>
    public DepthStencilStateDesc DepthStencilState;

    /// <summary>
    /// Gets or sets the BlendState state.
    /// </summary>
    public BlendStateDesc BlendState;

    /// <summary>
    /// The reference value to use when doing a stencil test.
    /// </summary>
    public int StencilReference;

    /// <summary>
    /// Array of blend factors, one for each RGBA component.
    /// </summary>
    public Vector4D<float>? BlendFactor;

    public static RenderStateDesc New(RasterizerStateDesc? rasterizerState = null,
                                      DepthStencilStateDesc? depthStencilState = null,
                                      BlendStateDesc? blendState = null,
                                      int stencilReference = 0,
                                      Vector4D<float>? blendFactor = null)
    {
        rasterizerState ??= RasterizerStateDesc.New();
        depthStencilState ??= DepthStencilStateDesc.New();
        blendState ??= BlendStateDesc.New();

        return new()
        {
            RasterizerState = rasterizerState.Value,
            DepthStencilState = depthStencilState.Value,
            BlendState = blendState.Value,
            StencilReference = stencilReference,
            BlendFactor = blendFactor
        };
    }
}