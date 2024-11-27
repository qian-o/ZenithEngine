using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct DepthStencilOperationDesc
{
    /// <summary>
    /// The stencil operation to perform when stencil testing fails.
    /// </summary>
    public StencilOperation StencilFailOperation { get; set; }

    /// <summary>
    /// The stencil operation to perform when stencil testing and depth testing both pass.
    /// </summary>
    public StencilOperation StencilPassOperation { get; set; }

    /// <summary>
    /// The stencil operation to perform when stencil testing passes and depth testing fails.
    /// </summary>
    public StencilOperation StencilDepthFailOperation { get; set; }

    /// <summary>
    /// The comparison operator used in the stencil test.
    /// </summary>
    public ComparisonFunction StencilFunction { get; set; }

    public static DepthStencilOperationDesc Default(StencilOperation stencilFailOperation = StencilOperation.Keep,
                                                    StencilOperation stencilPassOperation = StencilOperation.Keep,
                                                    StencilOperation stencilDepthFailOperation = StencilOperation.Keep,
                                                    ComparisonFunction stencilFunction = ComparisonFunction.Always)
    {
        return new()
        {
            StencilFailOperation = stencilFailOperation,
            StencilPassOperation = stencilPassOperation,
            StencilDepthFailOperation = stencilDepthFailOperation,
            StencilFunction = stencilFunction
        };
    }
}
