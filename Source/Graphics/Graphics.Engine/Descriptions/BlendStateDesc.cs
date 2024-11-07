namespace Graphics.Engine.Descriptions;

public struct BlendStateDesc
{
    /// <summary>
    /// Gets or sets the alpha to coverage enable.
    /// </summary>
    public bool AlphaToCoverageEnable { get; set; }

    /// <summary>
    /// Gets or sets the independent blend enable.
    /// </summary>
    public bool IndependentBlendEnable { get; set; }

    /// <summary>
    /// RenderTarget blend description 0 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget0 { get; set; }

    /// <summary>
    /// RenderTarget blend description 1 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget1 { get; set; }

    /// <summary>
    /// RenderTarget blend description 2 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget2 { get; set; }

    /// <summary>
    /// RenderTarget blend description 3 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget3 { get; set; }

    /// <summary>
    /// RenderTarget blend description 4 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget4 { get; set; }

    /// <summary>
    /// RenderTarget blend description 5 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget5 { get; set; }

    /// <summary>
    /// RenderTarget blend description 6 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget6 { get; set; }

    /// <summary>
    /// RenderTarget blend description 7 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget7 { get; set; }

    public static BlendStateDesc Default()
    {
        return new()
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = false,
            RenderTarget0 = BlendStateRenderTargetDesc.Default(),
            RenderTarget1 = BlendStateRenderTargetDesc.Default(),
            RenderTarget2 = BlendStateRenderTargetDesc.Default(),
            RenderTarget3 = BlendStateRenderTargetDesc.Default(),
            RenderTarget4 = BlendStateRenderTargetDesc.Default(),
            RenderTarget5 = BlendStateRenderTargetDesc.Default(),
            RenderTarget6 = BlendStateRenderTargetDesc.Default(),
            RenderTarget7 = BlendStateRenderTargetDesc.Default()
        };
    }
}
