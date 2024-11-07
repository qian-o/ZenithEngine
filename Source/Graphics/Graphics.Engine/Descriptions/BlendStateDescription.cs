namespace Graphics.Engine.Descriptions;

public struct BlendStateDescription
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
    public BlendStateRenderTargetDescription RenderTarget0 { get; set; }

    /// <summary>
    /// RenderTarget blend description 1 / 7.
    /// </summary>
    public BlendStateRenderTargetDescription RenderTarget1 { get; set; }

    /// <summary>
    /// RenderTarget blend description 2 / 7.
    /// </summary>
    public BlendStateRenderTargetDescription RenderTarget2 { get; set; }

    /// <summary>
    /// RenderTarget blend description 3 / 7.
    /// </summary>
    public BlendStateRenderTargetDescription RenderTarget3 { get; set; }

    /// <summary>
    /// RenderTarget blend description 4 / 7.
    /// </summary>
    public BlendStateRenderTargetDescription RenderTarget4 { get; set; }

    /// <summary>
    /// RenderTarget blend description 5 / 7.
    /// </summary>
    public BlendStateRenderTargetDescription RenderTarget5 { get; set; }

    /// <summary>
    /// RenderTarget blend description 6 / 7.
    /// </summary>
    public BlendStateRenderTargetDescription RenderTarget6 { get; set; }

    /// <summary>
    /// RenderTarget blend description 7 / 7.
    /// </summary>
    public BlendStateRenderTargetDescription RenderTarget7 { get; set; }

    public static BlendStateDescription Default()
    {
        return new BlendStateDescription
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = false,
            RenderTarget0 = BlendStateRenderTargetDescription.Default(),
            RenderTarget1 = BlendStateRenderTargetDescription.Default(),
            RenderTarget2 = BlendStateRenderTargetDescription.Default(),
            RenderTarget3 = BlendStateRenderTargetDescription.Default(),
            RenderTarget4 = BlendStateRenderTargetDescription.Default(),
            RenderTarget5 = BlendStateRenderTargetDescription.Default(),
            RenderTarget6 = BlendStateRenderTargetDescription.Default(),
            RenderTarget7 = BlendStateRenderTargetDescription.Default()
        };
    }
}
