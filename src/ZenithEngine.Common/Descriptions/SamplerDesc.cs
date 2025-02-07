using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct SamplerDesc(AddressMode addressModeU = AddressMode.Wrap,
                          AddressMode addressModeV = AddressMode.Wrap,
                          AddressMode addressModeW = AddressMode.Wrap,
                          SamplerFilter filter = SamplerFilter.MinLinearMagLinearMipLinear,
                          ComparisonFunction comparisonFunction = ComparisonFunction.Never,
                          uint maximumAnisotropy = 0,
                          uint minimumLod = 0,
                          uint maximumLod = uint.MaxValue,
                          int lodBias = 0,
                          SamplerBorderColor borderColor = SamplerBorderColor.TransparentBlack)
{
    public SamplerDesc()
    {
    }

    /// <summary>
    /// Mode to use for the U (or S) coordinate.
    /// </summary>
    public AddressMode AddressModeU = addressModeU;

    /// <summary>
    /// Mode to use for the V (or T) coordinate.
    /// </summary>
    public AddressMode AddressModeV = addressModeV;

    /// <summary>
    /// Mode to use for the W (or R) coordinate.
    /// </summary>
    public AddressMode AddressModeW = addressModeW;

    /// <summary>
    /// The filter used when sampling.
    /// </summary>
    public SamplerFilter Filter = filter;

    /// <summary>
    /// A function that compares sampled data against existing sampled data.
    /// </summary>
    public ComparisonFunction ComparisonFunction = comparisonFunction;

    /// <summary>
    /// The maximum anisotropy of the filter.
    /// </summary>
    public uint MaximumAnisotropy = maximumAnisotropy;

    /// <summary>
    /// The minimum level of detail.
    /// </summary>
    public uint MinimumLod = minimumLod;

    /// <summary>
    /// The maximum level of detail.
    /// </summary>
    public uint MaximumLod = maximumLod;

    /// <summary>
    /// The level of detail bias.
    /// </summary>
    public int LodBias = lodBias;

    /// <summary>
    /// The border color to use when sampling outside the texture.
    /// </summary>
    public SamplerBorderColor BorderColor = borderColor;
}
