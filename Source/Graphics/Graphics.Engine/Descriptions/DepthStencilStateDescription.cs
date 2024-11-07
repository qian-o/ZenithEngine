using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct DepthStencilStateDescription(bool depthEnabled,
                                           bool depthWriteEnabled,
                                           ComparisonFunction depthFunction,
                                           bool stencilEnabled,
                                           byte stencilReadMask,
                                           byte stencilWriteMask,
                                           DepthStencilOperationDescription frontFace,
                                           DepthStencilOperationDescription backFace)
{
    /// <summary>
    /// Controls whether depth testing is enabled.
    /// </summary>
    public bool DepthEnabled { get; set; } = depthEnabled;

    /// <summary>
    /// Controls whether new depth values are written to the depth buffer.
    /// </summary>
    public bool DepthWriteEnabled { get; set; } = depthWriteEnabled;

    /// <summary>
    /// The comparison function used to determine whether a new depth value should be written to the depth buffer.
    /// </summary>
    public ComparisonFunction DepthFunction { get; set; } = depthFunction;

    /// <summary>
    /// Controls whether the stencil test is enabled.
    /// </summary>
    public bool StencilEnabled { get; set; } = stencilEnabled;

    /// <summary>
    /// Identify a portion of the depth-stencil buffer for reading stencil data.
    /// </summary>
    public byte StencilReadMask { get; set; } = stencilReadMask;

    /// <summary>
    /// Identify a portion of the depth-stencil buffer for writing stencil data.
    /// </summary>
    public byte StencilWriteMask { get; set; } = stencilWriteMask;

    /// <summary>
    /// Identify how to use the results of the depth test and the stencil test for pixels
    /// whose surface normal is facing towards the camera.
    /// </summary>
    public DepthStencilOperationDescription FrontFace { get; set; } = frontFace;

    /// <summary>
    /// Identify how to use the results of the depth test and the stencil test for pixels
    /// whose surface normal is facing away from the camera.
    /// </summary>
    public DepthStencilOperationDescription BackFace { get; set; } = backFace;

    public static DepthStencilStateDescription Create(bool depthEnabled, bool stencilEnabled)
    {
        DepthStencilOperationDescription frontFace = new()
        {
            StencilFailOperation = StencilOperation.Keep,
            StencilDepthFailOperation = StencilOperation.Keep,
            StencilPassOperation = StencilOperation.Keep,
            StencilFunction = ComparisonFunction.Always
        };

        DepthStencilOperationDescription backFace = new()
        {
            StencilFailOperation = StencilOperation.Keep,
            StencilDepthFailOperation = StencilOperation.Keep,
            StencilPassOperation = StencilOperation.Keep,
            StencilFunction = ComparisonFunction.Always
        };

        return new DepthStencilStateDescription(depthEnabled,
                                                true,
                                                ComparisonFunction.LessEqual,
                                                stencilEnabled,
                                                byte.MaxValue,
                                                byte.MaxValue,
                                                frontFace,
                                                backFace);
    }
}
