﻿using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct SamplerDesc
{
    /// <summary>
    /// Mode to use for the U (or S) coordinate.
    /// </summary>
    public AddressMode AddressModeU;

    /// <summary>
    /// Mode to use for the V (or T) coordinate.
    /// </summary>
    public AddressMode AddressModeV;

    /// <summary>
    /// Mode to use for the W (or R) coordinate.
    /// </summary>
    public AddressMode AddressModeW;

    /// <summary>
    /// The filter used when sampling.
    /// </summary>
    public SamplerFilter Filter;

    /// <summary>
    /// A function that compares sampled data against existing sampled data.
    /// </summary>
    public ComparisonFunction ComparisonFunction;

    /// <summary>
    /// The maximum anisotropy of the filter.
    /// </summary>
    public uint MaximumAnisotropy;

    /// <summary>
    /// The minimum level of detail.
    /// </summary>
    public uint MinimumLod;

    /// <summary>
    /// The maximum level of detail.
    /// </summary>
    public uint MaximumLod;

    /// <summary>
    /// The level of detail bias.
    /// </summary>
    public int LodBias;

    /// <summary>
    /// The border color to use when sampling outside the texture.
    /// </summary>
    public SamplerBorderColor BorderColor;

    public static SamplerDesc New(AddressMode addressModeU = AddressMode.Wrap,
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
