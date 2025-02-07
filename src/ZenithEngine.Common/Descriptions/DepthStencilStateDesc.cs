using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct DepthStencilStateDesc(bool depthEnabled = true,
                                    bool depthWriteEnabled = true,
                                    ComparisonFunction depthFunction = ComparisonFunction.LessEqual,
                                    bool stencilEnabled = false,
                                    byte stencilReadMask = byte.MaxValue,
                                    byte stencilWriteMask = byte.MaxValue,
                                    DepthStencilOperationDesc? frontFace = null,
                                    DepthStencilOperationDesc? backFace = null)
{
    /// <summary>
    /// Controls whether depth testing is enabled.
    /// </summary>
    public bool DepthEnabled = depthEnabled;

    /// <summary>
    /// Controls whether new depth values are written to the depth buffer.
    /// </summary>
    public bool DepthWriteEnabled = depthWriteEnabled;

    /// <summary>
    /// The comparison function used to determine whether a new depth value should be written to the depth buffer.
    /// </summary>
    public ComparisonFunction DepthFunction = depthFunction;

    /// <summary>
    /// Controls whether the stencil test is enabled.
    /// </summary>
    public bool StencilEnabled = stencilEnabled;

    /// <summary>
    /// Identify a portion of the depth-stencil buffer for reading stencil data.
    /// </summary>
    public byte StencilReadMask = stencilReadMask;

    /// <summary>
    /// Identify a portion of the depth-stencil buffer for writing stencil data.
    /// </summary>
    public byte StencilWriteMask = stencilWriteMask;

    /// <summary>
    /// Identify how to use the results of the depth test and the stencil test for pixels
    /// whose surface normal is facing towards the camera.
    /// </summary>
    public DepthStencilOperationDesc FrontFace = frontFace ?? new();

    /// <summary>
    /// Identify how to use the results of the depth test and the stencil test for pixels
    /// whose surface normal is facing away from the camera.
    /// </summary>
    public DepthStencilOperationDesc BackFace = backFace ?? new();
}
