using Graphics.Core;

namespace Graphics.Vulkan;

public record struct DepthStencilStateDescription
{
    /// <summary>
    /// Controls whether depth testing is enabled.
    /// </summary>
    public bool DepthTestEnabled { get; set; }

    /// <summary>
    /// Controls whether new depth values are written to the depth buffer.
    /// </summary>
    public bool DepthWriteEnabled { get; set; }

    /// <summary>
    /// The comparison function used to determine whether a new depth value should be written to the depth buffer.
    /// </summary>
    public ComparisonKind DepthComparison { get; set; }

    /// <summary>
    /// Controls whether the stencil test is enabled.
    /// </summary>
    public bool StencilTestEnabled { get; set; }

    /// <summary>
    /// Controls how stencil tests are handled for pixels whose surface faces towards the camera.
    /// </summary>
    public StencilBehaviorDescription StencilFront { get; set; }

    /// <summary>
    /// Controls how stencil tests are handled for pixels whose surface faces away from the camera.
    /// </summary>
    public StencilBehaviorDescription StencilBack { get; set; }

    /// <summary>
    /// Controls the portion of the stencil buffer used for reading.
    /// </summary>
    public byte StencilReadMask { get; set; }

    /// <summary>
    /// Controls the portion of the stencil buffer used for writing.
    /// </summary>
    public byte StencilWriteMask { get; set; }

    /// <summary>
    /// The reference value to use when doing a stencil test.
    /// </summary>
    public uint StencilReference { get; set; }
}
