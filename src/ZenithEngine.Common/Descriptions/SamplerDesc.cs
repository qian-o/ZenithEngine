using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct SamplerDesc
{
    /// <summary>
    /// Mode to use for the U (or S) coordinate.
    /// </summary>
    public AddressMode AddressModeU { get; set; }

    /// <summary>
    /// Mode to use for the V (or T) coordinate.
    /// </summary>
    public AddressMode AddressModeV { get; set; }

    /// <summary>
    /// Mode to use for the W (or R) coordinate.
    /// </summary>
    public AddressMode AddressModeW { get; set; }

    /// <summary>
    /// The filter used when sampling.
    /// </summary>
    public SamplerFilter Filter { get; set; }

    /// <summary>
    /// A function that compares sampled data against existing sampled data.
    /// If null, comparison sampling is not used.
    /// </summary>
    public ComparisonFunction? ComparisonFunction { get; set; }

    /// <summary>
    /// The maximum anisotropy of the filter.
    /// </summary>
    public uint MaximumAnisotropy { get; set; }

    /// <summary>
    /// The minimum level of detail.
    /// </summary>
    public uint MinimumLod { get; set; }

    /// <summary>
    /// The maximum level of detail.
    /// </summary>
    public uint MaximumLod { get; set; }

    /// <summary>
    /// The level of detail bias.
    /// </summary>
    public int LodBias { get; set; }

    /// <summary>
    /// The border color to use when sampling outside the texture.
    /// </summary>
    public SamplerBorderColor BorderColor { get; set; }

    public static SamplerDesc Default(AddressMode addressModeU = AddressMode.Wrap,
                                      AddressMode addressModeV = AddressMode.Wrap,
                                      AddressMode addressModeW = AddressMode.Wrap,
                                      SamplerFilter filter = SamplerFilter.MinLinearMagLinearMipLinear,
                                      ComparisonFunction? comparisonFunction = null,
                                      uint maximumAnisotropy = 0,
                                      uint minimumLod = 0,
                                      uint maximumLod = uint.MaxValue,
                                      int lodBias = 0,
                                      SamplerBorderColor borderColor = SamplerBorderColor.TransparentBlack)
    {
        return new()
        {
            AddressModeU = addressModeU,
            AddressModeV = addressModeV,
            AddressModeW = addressModeW,
            Filter = filter,
            ComparisonFunction = comparisonFunction,
            MaximumAnisotropy = maximumAnisotropy,
            MinimumLod = minimumLod,
            MaximumLod = maximumLod,
            LodBias = lodBias,
            BorderColor = borderColor
        };
    }
}
