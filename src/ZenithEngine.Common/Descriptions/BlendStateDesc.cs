namespace ZenithEngine.Common.Descriptions;

public struct BlendStateDesc
{
    /// <summary>
    /// Specifies whether to use alpha-to-coverage as a multisampling technique when
    /// setting a pixel to a render target.
    /// </summary>
    public bool AlphaToCoverageEnabled { get; set; }

    /// <summary>
    /// Specifies whether to enable independent blending in simultaneous render targets.
    /// Set to TRUE to enable independent blending. If set to FALSE, only the RenderTarget[0]
    /// members are used; RenderTarget[1..7] are ignored.
    /// </summary>
    public bool IndependentBlendEnabled { get; set; }

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

    public static BlendStateDesc Default(bool alphaToCoverageEnabled = false,
                                         bool independentBlendEnabled = false,
                                         BlendStateRenderTargetDesc? renderTarget0 = null,
                                         BlendStateRenderTargetDesc? renderTarget1 = null,
                                         BlendStateRenderTargetDesc? renderTarget2 = null,
                                         BlendStateRenderTargetDesc? renderTarget3 = null,
                                         BlendStateRenderTargetDesc? renderTarget4 = null,
                                         BlendStateRenderTargetDesc? renderTarget5 = null,
                                         BlendStateRenderTargetDesc? renderTarget6 = null,
                                         BlendStateRenderTargetDesc? renderTarget7 = null)
    {
        renderTarget0 ??= BlendStateRenderTargetDesc.Default();
        renderTarget1 ??= BlendStateRenderTargetDesc.Default();
        renderTarget2 ??= BlendStateRenderTargetDesc.Default();
        renderTarget3 ??= BlendStateRenderTargetDesc.Default();
        renderTarget4 ??= BlendStateRenderTargetDesc.Default();
        renderTarget5 ??= BlendStateRenderTargetDesc.Default();
        renderTarget6 ??= BlendStateRenderTargetDesc.Default();
        renderTarget7 ??= BlendStateRenderTargetDesc.Default();

        return new()
        {
            AlphaToCoverageEnabled = alphaToCoverageEnabled,
            IndependentBlendEnabled = independentBlendEnabled,
            RenderTarget0 = renderTarget0.Value,
            RenderTarget1 = renderTarget1.Value,
            RenderTarget2 = renderTarget2.Value,
            RenderTarget3 = renderTarget3.Value,
            RenderTarget4 = renderTarget4.Value,
            RenderTarget5 = renderTarget5.Value,
            RenderTarget6 = renderTarget6.Value,
            RenderTarget7 = renderTarget7.Value
        };
    }
}
