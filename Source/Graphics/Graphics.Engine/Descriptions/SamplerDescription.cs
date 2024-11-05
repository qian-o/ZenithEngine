using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct SamplerDescription(AddressMode addressModeU,
                                 AddressMode addressModeV,
                                 AddressMode addressModeW,
                                 SamplerFilter filter,
                                 ComparisonKind? comparisonKind,
                                 uint maximumAnisotropy,
                                 uint minimumLod,
                                 uint maximumLod,
                                 int lodBias,
                                 SamplerBorderColor borderColor)
{
    public static readonly SamplerDescription Point = new(AddressMode.Wrap,
                                                          AddressMode.Wrap,
                                                          AddressMode.Wrap,
                                                          SamplerFilter.MinPointMagPointMipPoint,
                                                          null,
                                                          0,
                                                          0,
                                                          uint.MaxValue,
                                                          0,
                                                          SamplerBorderColor.TransparentBlack);

    public static readonly SamplerDescription Linear = new(AddressMode.Wrap,
                                                           AddressMode.Wrap,
                                                           AddressMode.Wrap,
                                                           SamplerFilter.MinLinearMagLinearMipLinear,
                                                           null,
                                                           0,
                                                           0,
                                                           uint.MaxValue,
                                                           0,
                                                           SamplerBorderColor.TransparentBlack);

    public static readonly SamplerDescription Aniso4x = new(AddressMode.Wrap,
                                                            AddressMode.Wrap,
                                                            AddressMode.Wrap,
                                                            SamplerFilter.Anisotropic,
                                                            null,
                                                            4,
                                                            0,
                                                            uint.MaxValue,
                                                            0,
                                                            SamplerBorderColor.TransparentBlack);

    /// <summary>
    /// Mode to use for the U (or S) coordinate.
    /// </summary>
    public AddressMode AddressModeU { get; set; } = addressModeU;

    /// <summary>
    /// Mode to use for the V (or T) coordinate.
    /// </summary>
    public AddressMode AddressModeV { get; set; } = addressModeV;

    /// <summary>
    /// Mode to use for the W (or R) coordinate.
    /// </summary>
    public AddressMode AddressModeW { get; set; } = addressModeW;

    /// <summary>
    /// The filter used when sampling.
    /// </summary>
    public SamplerFilter Filter { get; set; } = filter;

    /// <summary>
    /// An optional value controlling the kind of comparison to use when sampling. If null, comparison sampling is not used.
    /// </summary>
    public ComparisonKind? ComparisonKind { get; set; } = comparisonKind;

    /// <summary>
    /// The maximum anisotropy of the filter.
    /// </summary>
    public uint MaximumAnisotropy { get; set; } = maximumAnisotropy;

    /// <summary>
    /// The minimum level of detail.
    /// </summary>
    public uint MinimumLod { get; set; } = minimumLod;

    /// <summary>
    /// The maximum level of detail.
    /// </summary>
    public uint MaximumLod { get; set; } = maximumLod;

    /// <summary>
    /// The level of detail bias.
    /// </summary>
    public int LodBias { get; set; } = lodBias;

    /// <summary>
    /// The border color to use when sampling outside the texture.
    /// </summary>
    public SamplerBorderColor BorderColor { get; set; } = borderColor;
}
