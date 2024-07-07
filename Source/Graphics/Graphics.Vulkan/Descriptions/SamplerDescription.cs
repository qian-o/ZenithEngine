using Graphics.Core;

namespace Graphics.Vulkan;

public struct SamplerDescription(SamplerAddressMode addressModeU,
                                 SamplerAddressMode addressModeV,
                                 SamplerAddressMode addressModeW,
                                 SamplerFilter filter,
                                 ComparisonKind? comparisonKind,
                                 uint maximumAnisotropy,
                                 uint minimumLod,
                                 uint maximumLod,
                                 int lodBias,
                                 SamplerBorderColor borderColor) : IEquatable<SamplerDescription>
{
    public static readonly SamplerDescription Point = new(SamplerAddressMode.Wrap,
                                                          SamplerAddressMode.Wrap,
                                                          SamplerAddressMode.Wrap,
                                                          SamplerFilter.MinPointMagPointMipPoint,
                                                          null,
                                                          0,
                                                          0,
                                                          uint.MaxValue,
                                                          0,
                                                          SamplerBorderColor.TransparentBlack);

    public static readonly SamplerDescription Linear = new(SamplerAddressMode.Wrap,
                                                           SamplerAddressMode.Wrap,
                                                           SamplerAddressMode.Wrap,
                                                           SamplerFilter.MinLinearMagLinearMipLinear,
                                                           null,
                                                           0,
                                                           0,
                                                           uint.MaxValue,
                                                           0,
                                                           SamplerBorderColor.TransparentBlack);

    public static readonly SamplerDescription Aniso4x = new(SamplerAddressMode.Wrap,
                                                            SamplerAddressMode.Wrap,
                                                            SamplerAddressMode.Wrap,
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
    public SamplerAddressMode AddressModeU { get; set; } = addressModeU;

    /// <summary>
    /// Mode to use for the V (or T) coordinate.
    /// </summary>
    public SamplerAddressMode AddressModeV { get; set; } = addressModeV;

    /// <summary>
    /// Mode to use for the W (or R) coordinate.
    /// </summary>
    public SamplerAddressMode AddressModeW { get; set; } = addressModeW;

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

    public readonly bool Equals(SamplerDescription other)
    {
        return AddressModeU == other.AddressModeU
               && AddressModeV == other.AddressModeV
               && AddressModeW == other.AddressModeW
               && Filter == other.Filter
               && ComparisonKind == other.ComparisonKind
               && MaximumAnisotropy == other.MaximumAnisotropy
               && MinimumLod == other.MinimumLod
               && MaximumLod == other.MaximumLod
               && LodBias == other.LodBias
               && BorderColor == other.BorderColor;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(AddressModeU.GetHashCode(),
                                  AddressModeV.GetHashCode(),
                                  AddressModeW.GetHashCode(),
                                  Filter.GetHashCode(),
                                  ComparisonKind.GetHashCode(),
                                  MaximumAnisotropy.GetHashCode(),
                                  MinimumLod.GetHashCode(),
                                  MaximumLod.GetHashCode(),
                                  LodBias.GetHashCode(),
                                  BorderColor.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is SamplerDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"AddressModeU: {AddressModeU}, AddressModeV: {AddressModeV}, AddressModeW: {AddressModeW}, Filter: {Filter}, ComparisonKind: {ComparisonKind}, MaximumAnisotropy: {MaximumAnisotropy}, MinimumLod: {MinimumLod}, MaximumLod: {MaximumLod}, LodBias: {LodBias}, BorderColor: {BorderColor}";
    }

    public static bool operator ==(SamplerDescription left, SamplerDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SamplerDescription left, SamplerDescription right)
    {
        return !(left == right);
    }
}
