using Graphics.Core;

namespace Graphics.Vulkan;

public readonly record struct StencilBehaviorDescription
{
    public StencilBehaviorDescription(StencilOperation fail,
                                      StencilOperation pass,
                                      StencilOperation depthFail,
                                      ComparisonKind comparison)
    {
        Fail = fail;
        Pass = pass;
        DepthFail = depthFail;
        Comparison = comparison;
    }

    /// <summary>
    /// The operation performed on samples that fail the stencil test.
    /// </summary>
    public StencilOperation Fail { get; }

    /// <summary>
    /// The operation performed on samples that pass the stencil test.
    /// </summary>
    public StencilOperation Pass { get; }

    /// <summary>
    /// The operation performed on samples that pass the stencil test but fail the depth test.
    /// </summary>
    public StencilOperation DepthFail { get; }

    /// <summary>
    /// The comparison operator used in the stencil test.
    /// </summary>
    public ComparisonKind Comparison { get; }
}
