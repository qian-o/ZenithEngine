using ZenithEngine.Common.Descriptions;

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
    }
}
