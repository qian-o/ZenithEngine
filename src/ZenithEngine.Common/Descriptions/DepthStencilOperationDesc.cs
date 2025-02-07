using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct DepthStencilOperationDesc
{
    /// <summary>
    /// The stencil operation to perform when stencil testing fails.
    /// </summary>
    public StencilOperation StencilFailOperation;

    /// <summary>
    /// The stencil operation to perform when stencil testing passes and depth testing fails.
    /// </summary>
    public StencilOperation StencilDepthFailOperation;

    /// <summary>
    /// The stencil operation to perform when stencil testing and depth testing both pass.
    /// </summary>
    public StencilOperation StencilPassOperation;

    /// <summary>
    /// The comparison operator used in the stencil test.
    /// </summary>
    public ComparisonFunction StencilFunction;

    public static DepthStencilOperationDesc New(StencilOperation stencilFailOperation = StencilOperation.Keep,
                                                StencilOperation stencilDepthFailOperation = StencilOperation.Keep,
                                                StencilOperation stencilPassOperation = StencilOperation.Keep,
                                                ComparisonFunction stencilFunction = ComparisonFunction.Always)
    {
        return new()
        {
            StencilFailOperation = stencilFailOperation,
            StencilDepthFailOperation = stencilDepthFailOperation,
            StencilPassOperation = stencilPassOperation,
            StencilFunction = stencilFunction
        };
    }
}
