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
        Opaque.RenderTarget0.BlendEnabled = false;
        Opaque.RenderTarget0.SourceBlendColor = Blend.One;
        Opaque.RenderTarget0.DestinationBlendColor = Blend.Zero;

        AlphaBlend = BlendStateDesc.Default();
        AlphaBlend.RenderTarget0.BlendEnabled = true;
        AlphaBlend.RenderTarget0.SourceBlendColor = Blend.One;
        AlphaBlend.RenderTarget0.DestinationBlendColor = Blend.InverseSourceAlpha;
        AlphaBlend.RenderTarget0.SourceBlendAlpha = Blend.One;
        AlphaBlend.RenderTarget0.DestinationBlendAlpha = Blend.InverseSourceAlpha;

        Additive = BlendStateDesc.Default();
        Additive.RenderTarget0.BlendEnabled = true;
        Additive.RenderTarget0.BlendOperationColor = BlendOperation.Add;
        Additive.RenderTarget0.BlendOperationAlpha = BlendOperation.Add;
        Additive.RenderTarget0.SourceBlendColor = Blend.One;
        Additive.RenderTarget0.DestinationBlendColor = Blend.One;
        Additive.RenderTarget0.SourceBlendAlpha = Blend.One;
        Additive.RenderTarget0.DestinationBlendAlpha = Blend.One;

        Multiplicative = BlendStateDesc.Default();
        Multiplicative.RenderTarget0.BlendEnabled = true;
        Multiplicative.RenderTarget0.BlendOperationColor = BlendOperation.Add;
        Multiplicative.RenderTarget0.BlendOperationAlpha = BlendOperation.Add;
        Multiplicative.RenderTarget0.SourceBlendColor = Blend.DestinationColor;
        Multiplicative.RenderTarget0.DestinationBlendColor = Blend.InverseSourceAlpha;
        Multiplicative.RenderTarget0.SourceBlendAlpha = Blend.One;
        Multiplicative.RenderTarget0.DestinationBlendAlpha = Blend.One;

        NonPremultiplied = BlendStateDesc.Default();
        NonPremultiplied.RenderTarget0.BlendEnabled = true;
        NonPremultiplied.RenderTarget0.SourceBlendColor = Blend.SourceAlpha;
        NonPremultiplied.RenderTarget0.DestinationBlendColor = Blend.InverseSourceAlpha;
    }
}
