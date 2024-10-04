namespace Graphics.Core;

public enum HitGroupType
{
    /// <summary>
    /// The hit group indicates a shader group with a single
    /// </summary>
    General,

    /// <summary>
    /// The hit group uses a list of triangles to calculate ray hits. Hit groups that
    /// use triangles can’t contain an intersection shader.
    /// </summary>
    Triangles,

    /// <summary>
    /// The hit group uses a procedural primitive within a bounding box to calculate
    /// ray hits. Hit groups that use procedural primitives must contain an intersection
    /// shader.
    /// </summary>
    Procedural
}
