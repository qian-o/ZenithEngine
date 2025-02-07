using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct DepthStencilStateDesc
{
    /// <summary>
    /// Controls whether depth testing is enabled.
    /// </summary>
    public bool DepthEnabled;

    /// <summary>
    /// Controls whether new depth values are written to the depth buffer.
    /// </summary>
    public bool DepthWriteEnabled;

    /// <summary>
    /// The comparison function used to determine whether a new depth value should be written to the depth buffer.
    /// </summary>
    public ComparisonFunction DepthFunction;

    /// <summary>
    /// Controls whether the stencil test is enabled.
    /// </summary>
    public bool StencilEnabled;

    /// <summary>
    /// Identify a portion of the depth-stencil buffer for reading stencil data.
    /// </summary>
    public byte StencilReadMask;

    /// <summary>
    /// Identify a portion of the depth-stencil buffer for writing stencil data.
    /// </summary>
    public byte StencilWriteMask;

    /// <summary>
    /// Identify how to use the results of the depth test and the stencil test for pixels
    /// whose surface normal is facing towards the camera.
    /// </summary>
    public DepthStencilOperationDesc FrontFace;

    /// <summary>
    /// Identify how to use the results of the depth test and the stencil test for pixels
    /// whose surface normal is facing away from the camera.
    /// </summary>
    public DepthStencilOperationDesc BackFace;

    public static DepthStencilStateDesc New(bool depthEnabled = true,
                                            bool depthWriteEnabled = true,
                                            ComparisonFunction depthFunction = ComparisonFunction.LessEqual,
                                            bool stencilEnabled = false,
                                            byte stencilReadMask = byte.MaxValue,
                                            byte stencilWriteMask = byte.MaxValue,
                                            DepthStencilOperationDesc? frontFace = null,
                                            DepthStencilOperationDesc? backFace = null)
    {
        frontFace ??= DepthStencilOperationDesc.New();
        backFace ??= DepthStencilOperationDesc.New();

        return new()
        {
            DepthEnabled = depthEnabled,
            DepthWriteEnabled = depthWriteEnabled,
            DepthFunction = depthFunction,
            StencilEnabled = stencilEnabled,
            StencilReadMask = stencilReadMask,
            StencilWriteMask = stencilWriteMask,
            FrontFace = frontFace.Value,
            BackFace = backFace.Value
        };
    }
}
