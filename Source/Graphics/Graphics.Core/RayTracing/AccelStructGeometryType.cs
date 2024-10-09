namespace Graphics.Core.RayTracing;

public enum AccelStructGeometryType : byte
{
    None,

    /// <summary>
    /// Geometry is opaque.
    /// </summary>
    Opaque,

    /// <summary>
    /// Geometry is no duplicate.
    /// </summary>
    NoDuplicateAnyHitInvocation
}
