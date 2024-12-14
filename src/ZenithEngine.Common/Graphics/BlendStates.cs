using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public static class BlendStates
{
    /// <summary>
    /// Not blended.
    /// </summary>
    public static readonly BlendStateDesc Opaque;

    /// <summary>
    /// Pre-multiplied alpha blending.
    /// </summary>
    public static readonly BlendStateDesc AlphaBlend;

    /// <summary>
    /// Additive alpha blending.
    /// </summary>
    public static readonly BlendStateDesc Additive;

    /// <summary>
    /// Additive alpha blending effect.
    /// </summary>
    public static readonly BlendStateDesc Multiplicative;

    /// <summary>
    /// Non-premultiplied alpha blending.
    /// </summary>
    public static readonly BlendStateDesc NonPremultiplied;

    static BlendStates()
    {
        Opaque = BlendStateDesc.Default();

        AlphaBlend = BlendStateDesc.Default();
        AlphaBlend.RenderTarget0 = BlendStateRenderTargetDesc.Default
        (
            blendEnabled: true,
            sourceBlendColor: Blend.SourceAlpha,
            destinationBlendColor: Blend.InverseSourceAlpha,
            sourceBlendAlpha: Blend.SourceAlpha,
            destinationBlendAlpha: Blend.InverseSourceAlpha
        );

        Additive = BlendStateDesc.Default();
        Additive.RenderTarget0 = BlendStateRenderTargetDesc.Default
        (
            blendEnabled: true,
            destinationBlendColor: Blend.One,
            destinationBlendAlpha: Blend.One
        );

        Multiplicative = BlendStateDesc.Default();
        Multiplicative.RenderTarget0 = BlendStateRenderTargetDesc.Default
        (
            blendEnabled: true,
            sourceBlendColor: Blend.DestinationColor,
            sourceBlendAlpha: Blend.One,
            destinationBlendColor: Blend.InverseSourceAlpha,
            destinationBlendAlpha: Blend.One
        );

        NonPremultiplied = BlendStateDesc.Default();
        NonPremultiplied.RenderTarget0 = BlendStateRenderTargetDesc.Default
        (
            blendEnabled: true,
            sourceBlendColor: Blend.SourceAlpha,
            destinationBlendColor: Blend.InverseSourceAlpha
        );
    }
}
