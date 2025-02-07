using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct BlendStateRenderTargetDesc(bool blendEnabled = false,
                                         Blend sourceBlendColor = Blend.One,
                                         Blend destinationBlendColor = Blend.Zero,
                                         BlendOperation blendOperationColor = BlendOperation.Add,
                                         Blend sourceBlendAlpha = Blend.One,
                                         Blend destinationBlendAlpha = Blend.Zero,
                                         BlendOperation blendOperationAlpha = BlendOperation.Add,
                                         ColorWriteChannels colorWriteChannels = ColorWriteChannels.All)
{
    /// <summary>
    /// Controls whether blending is enabled for the color attachment.
    /// </summary>
    public bool BlendEnabled = blendEnabled;

    /// <summary>
    /// This blend option specifies the operation to perform on the RGB value that the
    /// pixel shader outputs. The BlendOp member defines how to combine the SrcBlend
    /// and DestBlend operations.
    /// </summary>
    public Blend SourceBlendColor = sourceBlendColor;

    /// <summary>
    /// This blend option specifies the operation to perform on the current RGB value
    /// in the render target. The BlendOp member defines how to combine the SrcBlend
    /// and DestBlend operations.
    /// </summary>
    public Blend DestinationBlendColor = destinationBlendColor;

    /// <summary>
    /// This blend operation defines how to combine the SrcBlend and DestBlend operations.
    /// </summary>
    public BlendOperation BlendOperationColor = blendOperationColor;

    /// <summary>
    /// This blend option specifies the operation to perform on the alpha value that
    /// the pixel shader outputs. Blend options that end in _COLOR are not allowed. The
    /// BlendOpAlpha member defines how to combine the SrcBlendAlpha and DestBlendAlpha
    /// operations.
    /// </summary>
    public Blend SourceBlendAlpha = sourceBlendAlpha;

    /// <summary>
    /// This blend option specifies the operation to perform on the current alpha value
    /// in the render target. Blend options that end in _COLOR are not allowed. The BlendOpAlpha
    /// member defines how to combine the SrcBlendAlpha and DestBlendAlpha operations.
    /// </summary>
    public Blend DestinationBlendAlpha = destinationBlendAlpha;

    /// <summary>
    /// This blend operation defines how to combine the SrcBlendAlpha and DestBlendAlpha operations.
    /// </summary>
    public BlendOperation BlendOperationAlpha = blendOperationAlpha;

    /// <summary>
    /// A write mask.
    /// </summary>
    public ColorWriteChannels ColorWriteChannels = colorWriteChannels;
}
