namespace Graphics.Core;

public enum AccelStructInstanceType : byte
{
    None,

    /// <summary>
    /// Disables front/back face culling for this instance.
    /// </summary>
    TriangleCullDisable,

    /// <summary>
    /// This flag reverses front and back facings.
    /// </summary>
    TriangleFrontCounterClockwise,

    /// <summary>
    /// Applied to all the geometries in the bottom-level acceleration structure referenced by the instance.
    /// </summary>
    ForceOpaque,

    /// <summary>
    /// Applied to any of the geometries in the bottom-level acceleration structure referenced by the instance.
    /// </summary>
    ForceNoOpaque
}
