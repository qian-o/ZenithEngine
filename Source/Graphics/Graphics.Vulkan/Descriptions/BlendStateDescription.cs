using Graphics.Core;

namespace Graphics.Vulkan;

public struct BlendStateDescription(RgbaFloat blendFactor,
                                    BlendAttachmentDescription[] attachmentStates,
                                    bool alphaToCoverageEnabled) : IEquatable<BlendStateDescription>
{
    public static readonly BlendStateDescription SingleOverrideBlend = new(default,
                                                                           BlendAttachmentDescription.OverrideBlend);

    public static readonly BlendStateDescription SingleAlphaBlend = new(default,
                                                                        BlendAttachmentDescription.AlphaBlend);

    public static readonly BlendStateDescription SingleAdditiveBlend = new(default,
                                                                           BlendAttachmentDescription.AdditiveBlend);

    public static readonly BlendStateDescription SingleDisabled = new(default,
                                                                       BlendAttachmentDescription.Disabled);

    public static readonly BlendStateDescription Empty = new(default, []);

    public BlendStateDescription(RgbaFloat blendFactor,
                                 params BlendAttachmentDescription[] attachmentStates) : this(blendFactor,
                                                                                              attachmentStates,
                                                                                              false)
    {
    }

    public BlendStateDescription(RgbaFloat blendFactor,
                                 bool alphaToCoverageEnabled,
                                 params BlendAttachmentDescription[] attachmentStates) : this(blendFactor,
                                                                                              attachmentStates,
                                                                                              alphaToCoverageEnabled)
    {
    }

    /// <summary>
    /// A constant blend color used by all blend operations.
    /// </summary>
    public RgbaFloat BlendFactor { get; set; } = blendFactor;

    /// <summary>
    /// The Array describes the blend state for each render target.
    /// </summary>
    public BlendAttachmentDescription[] AttachmentStates { get; set; } = attachmentStates;

    /// <summary>
    /// Enables alpha-to-coverage, which causes a fragment's alpha value to be used when determining multi-sample coverage.
    /// </summary>
    public bool AlphaToCoverageEnabled { get; set; } = alphaToCoverageEnabled;

    public readonly bool Equals(BlendStateDescription other)
    {
        return BlendFactor == other.BlendFactor
               && AttachmentStates.SequenceEqual(other.AttachmentStates)
               && AlphaToCoverageEnabled == other.AlphaToCoverageEnabled;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(BlendFactor.GetHashCode(),
                                  AttachmentStates.GetHashCode(),
                                  AlphaToCoverageEnabled.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is BlendStateDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"BlendFactor: {BlendFactor}, AttachmentStates: {AttachmentStates}, AlphaToCoverageEnabled: {AlphaToCoverageEnabled}";
    }

    public static bool operator ==(BlendStateDescription left, BlendStateDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BlendStateDescription left, BlendStateDescription right)
    {
        return !(left == right);
    }
}
