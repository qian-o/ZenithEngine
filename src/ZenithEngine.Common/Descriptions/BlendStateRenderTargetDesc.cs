using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct BlendStateRenderTargetDesc
{
    /// <summary>
    /// Controls whether blending is enabled for the color attachment.
    /// </summary>
    public bool BlendEnabled { get; set; }

    /// <summary>
    /// This blend option specifies the operation to perform on the RGB value that the
    /// pixel shader outputs. The BlendOp member defines how to combine the SrcBlend
    /// and DestBlend operations.
    /// </summary>
    public Blend SourceBlendColor { get; set; }

    /// <summary>
    /// This blend option specifies the operation to perform on the current RGB value
    /// in the render target. The BlendOp member defines how to combine the SrcBlend
    /// and DestBlend operations.
    /// </summary>
    public Blend DestinationBlendColor { get; set; }

    /// <summary>
    /// This blend operation defines how to combine the SrcBlend and DestBlend operations.
    /// </summary>
    public BlendOperation BlendOperationColor { get; set; }

    /// <summary>
    /// This blend option specifies the operation to perform on the alpha value that
    /// the pixel shader outputs. Blend options that end in _COLOR are not allowed. The
    /// BlendOpAlpha member defines how to combine the SrcBlendAlpha and DestBlendAlpha
    /// operations.
    /// </summary>
    public Blend SourceBlendAlpha { get; set; }

    /// <summary>
    /// This blend option specifies the operation to perform on the current alpha value
    /// in the render target. Blend options that end in _COLOR are not allowed. The BlendOpAlpha
    /// member defines how to combine the SrcBlendAlpha and DestBlendAlpha operations.
    /// </summary>
    public Blend DestinationBlendAlpha { get; set; }

    /// <summary>
    /// This blend operation defines how to combine the SrcBlendAlpha and DestBlendAlpha operations.
    /// </summary>
    public BlendOperation BlendOperationAlpha { get; set; }

    /// <summary>
    /// A write mask.
    /// </summary>
    public ColorWriteChannels ColorWriteChannels { get; set; }

    public static BlendStateRenderTargetDesc Default(bool blendEnabled = false,
                                                     Blend sourceBlendColor = Blend.One,
                                                     Blend destinationBlendColor = Blend.Zero,
                                                     BlendOperation blendOperationColor = BlendOperation.Add,
                                                     Blend sourceBlendAlpha = Blend.One,
                                                     Blend destinationBlendAlpha = Blend.Zero,
                                                     BlendOperation blendOperationAlpha = BlendOperation.Add,
                                                     ColorWriteChannels colorWriteChannels = ColorWriteChannels.All)
    {
        return new()
        {
            BlendEnabled = blendEnabled,
            SourceBlendColor = sourceBlendColor,
            DestinationBlendColor = destinationBlendColor,
            BlendOperationColor = blendOperationColor,
            SourceBlendAlpha = sourceBlendAlpha,
            DestinationBlendAlpha = destinationBlendAlpha,
            BlendOperationAlpha = blendOperationAlpha,
            ColorWriteChannels = colorWriteChannels
        };
    }
}
