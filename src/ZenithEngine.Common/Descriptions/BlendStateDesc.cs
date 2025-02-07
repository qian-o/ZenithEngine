namespace ZenithEngine.Common.Descriptions;

public struct BlendStateDesc(bool alphaToCoverageEnabled = false,
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
    public BlendStateDesc() : this(false, false, null, null, null, null, null, null, null)
    {
    }

    /// <summary>
    /// Specifies whether to use alpha-to-coverage as a multisampling technique when
    /// setting a pixel to a render target.
    /// </summary>
    public bool AlphaToCoverageEnabled = alphaToCoverageEnabled;

    /// <summary>
    /// Specifies whether to enable independent blending in simultaneous render targets.
    /// Set to TRUE to enable independent blending. If set to FALSE, only the RenderTarget[0]
    /// members are used; RenderTarget[1..7] are ignored.
    /// </summary>
    public bool IndependentBlendEnabled = independentBlendEnabled;

    /// <summary>
    /// RenderTarget blend description 0 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget0 = renderTarget0 ?? new();

    /// <summary>
    /// RenderTarget blend description 1 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget1 = renderTarget1 ?? new();

    /// <summary>
    /// RenderTarget blend description 2 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget2 = renderTarget2 ?? new();

    /// <summary>
    /// RenderTarget blend description 3 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget3 = renderTarget3 ?? new();

    /// <summary>
    /// RenderTarget blend description 4 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget4 = renderTarget4 ?? new();

    /// <summary>
    /// RenderTarget blend description 5 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget5 = renderTarget5 ?? new();

    /// <summary>
    /// RenderTarget blend description 6 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget6 = renderTarget6 ?? new();

    /// <summary>
    /// RenderTarget blend description 7 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget7 = renderTarget7 ?? new();
}
