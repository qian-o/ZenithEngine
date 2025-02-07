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
        Opaque = new();

        AlphaBlend = new()
        {
            RenderTarget0 = new()
            {
                BlendEnabled = true,
                SourceBlendColor = Blend.SourceAlpha,
                DestinationBlendColor = Blend.InverseSourceAlpha,
                SourceBlendAlpha = Blend.SourceAlpha,
                DestinationBlendAlpha = Blend.InverseSourceAlpha
            }
        };

        Additive = new()
        {
            RenderTarget0 = new()
            {
                BlendEnabled = true,
                DestinationBlendColor = Blend.One,
                DestinationBlendAlpha = Blend.One
            }
        };

        Multiplicative = new()
        {
            RenderTarget0 = new()
            {
                BlendEnabled = true,
                SourceBlendColor = Blend.DestinationColor,
                DestinationBlendColor = Blend.InverseSourceAlpha,
                SourceBlendAlpha = Blend.One,
                DestinationBlendAlpha = Blend.One
            }
        };

        NonPremultiplied = new()
        {
            RenderTarget0 = new()
            {
                BlendEnabled = true,
                SourceBlendColor = Blend.SourceAlpha,
                DestinationBlendColor = Blend.InverseSourceAlpha
            }
        };
    }
}
