namespace Graphics.Core.RayTracing;

[Flags]
public enum GeometryMask : byte
{
    /// <summary>
    /// No flags.
    /// </summary>
    None = 0,

    /// <summary>
    /// Geometry is opaque.
    /// </summary>
    Opaque = 1 << 0,

    /// <summary>
    /// Geometry is no duplicate.
    /// </summary>
    NoDuplicateAnyHitInvocation = 1 << 1
}
