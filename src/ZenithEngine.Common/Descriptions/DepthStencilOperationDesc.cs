using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct DepthStencilOperationDesc(StencilOperation stencilFailOperation = StencilOperation.Keep,
                                        StencilOperation stencilDepthFailOperation = StencilOperation.Keep,
                                        StencilOperation stencilPassOperation = StencilOperation.Keep,
                                        ComparisonFunction stencilFunction = ComparisonFunction.Always)
{
    public DepthStencilOperationDesc()
    {
    }

    /// <summary>
    /// The stencil operation to perform when stencil testing fails.
    /// </summary>
    public StencilOperation StencilFailOperation = stencilFailOperation;

    /// <summary>
    /// The stencil operation to perform when stencil testing passes and depth testing fails.
    /// </summary>
    public StencilOperation StencilDepthFailOperation = stencilDepthFailOperation;

    /// <summary>
    /// The stencil operation to perform when stencil testing and depth testing both pass.
    /// </summary>
    public StencilOperation StencilPassOperation = stencilPassOperation;

    /// <summary>
    /// The comparison operator used in the stencil test.
    /// </summary>
    public ComparisonFunction StencilFunction = stencilFunction;
}
