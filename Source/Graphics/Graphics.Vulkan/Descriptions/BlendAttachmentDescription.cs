using Graphics.Core;

namespace Graphics.Vulkan;

public struct BlendAttachmentDescription(bool blendEnabled,
                                         ColorWriteMask colorWriteMask,
                                         BlendFactor sourceColorFactor,
                                         BlendFactor destinationColorFactor,
                                         BlendFunction colorFunction,
                                         BlendFactor sourceAlphaFactor,
                                         BlendFactor destinationAlphaFactor,
                                         BlendFunction alphaFunction) : IEquatable<BlendAttachmentDescription>
{
    public static readonly BlendAttachmentDescription OverrideBlend = new(false,
                                                                          BlendFactor.One,
                                                                          BlendFactor.Zero,
                                                                          BlendFunction.Add,
                                                                          BlendFactor.One,
                                                                          BlendFactor.Zero,
                                                                          BlendFunction.Add);

    public static readonly BlendAttachmentDescription AlphaBlend = new(true,
                                                                       BlendFactor.SourceAlpha,
                                                                       BlendFactor.InverseSourceAlpha,
                                                                       BlendFunction.Add,
                                                                       BlendFactor.SourceAlpha,
                                                                       BlendFactor.InverseSourceAlpha,
                                                                       BlendFunction.Add);

    public static readonly BlendAttachmentDescription AdditiveBlend = new(true,
                                                                          BlendFactor.SourceAlpha,
                                                                          BlendFactor.One,
                                                                          BlendFunction.Add,
                                                                          BlendFactor.SourceAlpha,
                                                                          BlendFactor.One,
                                                                          BlendFunction.Add);

    public static readonly BlendAttachmentDescription Disabled = new(false,
                                                                     BlendFactor.One,
                                                                     BlendFactor.Zero,
                                                                     BlendFunction.Add,
                                                                     BlendFactor.One,
                                                                     BlendFactor.Zero,
                                                                     BlendFunction.Add);

    public BlendAttachmentDescription(bool blendEnabled,
                                      BlendFactor sourceColorFactor,
                                      BlendFactor destinationColorFactor,
                                      BlendFunction colorFunction,
                                      BlendFactor sourceAlphaFactor,
                                      BlendFactor destinationAlphaFactor,
                                      BlendFunction alphaFunction) : this(blendEnabled,
                                                                          ColorWriteMask.All,
                                                                          sourceColorFactor,
                                                                          destinationColorFactor,
                                                                          colorFunction,
                                                                          sourceAlphaFactor,
                                                                          destinationAlphaFactor,
                                                                          alphaFunction)
    {
    }

    /// <summary>
    /// Controls whether blending is enabled for the color attachment.
    /// </summary>
    public bool BlendEnabled { get; set; } = blendEnabled;

    /// <summary>
    /// Controls which components of the color will be written to the framebuffer.
    /// </summary>
    public ColorWriteMask ColorWriteMask { get; set; } = colorWriteMask;

    /// <summary>
    /// Controls the source color's influence on the blend result.
    /// </summary>
    public BlendFactor SourceColorFactor { get; set; } = sourceColorFactor;

    /// <summary>
    /// Controls the destination color's influence on the blend result.
    /// </summary>
    public BlendFactor DestinationColorFactor { get; set; } = destinationColorFactor;

    /// <summary>
    /// Controls the function used to combine the source and destination color factors.
    /// </summary>
    public BlendFunction ColorFunction { get; set; } = colorFunction;

    /// <summary>
    /// Controls the source alpha's influence on the blend result.
    /// </summary>
    public BlendFactor SourceAlphaFactor { get; set; } = sourceAlphaFactor;

    /// <summary>
    /// Controls the destination alpha's influence on the blend result.
    /// </summary>
    public BlendFactor DestinationAlphaFactor { get; set; } = destinationAlphaFactor;

    /// <summary>
    /// Controls the function used to combine the source and destination alpha factors.
    /// </summary>
    public BlendFunction AlphaFunction { get; set; } = alphaFunction;

    public readonly bool Equals(BlendAttachmentDescription other)
    {
        return BlendEnabled == other.BlendEnabled
               && ColorWriteMask == other.ColorWriteMask
               && SourceColorFactor == other.SourceColorFactor
               && DestinationColorFactor == other.DestinationColorFactor
               && ColorFunction == other.ColorFunction
               && SourceAlphaFactor == other.SourceAlphaFactor
               && DestinationAlphaFactor == other.DestinationAlphaFactor
               && AlphaFunction == other.AlphaFunction;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(BlendEnabled.GetHashCode(),
                                  ColorWriteMask.GetHashCode(),
                                  SourceColorFactor.GetHashCode(),
                                  DestinationColorFactor.GetHashCode(),
                                  ColorFunction.GetHashCode(),
                                  SourceAlphaFactor.GetHashCode(),
                                  DestinationAlphaFactor.GetHashCode(),
                                  AlphaFunction.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is BlendAttachmentDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"BlendEnabled: {BlendEnabled}, ColorWriteMask: {ColorWriteMask}, SourceColorFactor: {SourceColorFactor}, DestinationColorFactor: {DestinationColorFactor}, ColorFunction: {ColorFunction}, SourceAlphaFactor: {SourceAlphaFactor}, DestinationAlphaFactor: {DestinationAlphaFactor}, AlphaFunction: {AlphaFunction}";
    }

    public static bool operator ==(BlendAttachmentDescription left, BlendAttachmentDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BlendAttachmentDescription left, BlendAttachmentDescription right)
    {
        return !(left == right);
    }
}
