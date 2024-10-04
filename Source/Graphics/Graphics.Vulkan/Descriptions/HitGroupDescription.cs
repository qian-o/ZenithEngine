namespace Graphics.Vulkan.Descriptions;

public record struct HitGroupDescription
{
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

    /// <summary>
    /// A value from the HitGroupType enumeration specifying the type of the hit group.
    /// </summary>
    public HitGroupType Type { get; set; }

    /// <summary>
    /// The name of the hit group.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Optional name of the general shader associated with the hit group. This field
    /// can be used with all hit group types.
    /// </summary>
    public string GeneralEntryPoint { get; set; }

    /// <summary>
    /// Optional name of the closest-hit shader associated with the hit group. This field
    /// can be used with all hit group types.
    /// </summary>
    public string ClosestHitEntryPoint { get; set; }

    /// <summary>
    /// Optional name of the any-hit shader associated with the hit group. This field
    /// can be used with all hit group types.
    /// </summary>
    public string AnyHitEntryPoint { get; set; }

    /// <summary>
    /// Optional name of the intersection shader associated with the hit group. This
    /// field can only be used with hit groups of type procedural primitive.
    /// </summary>
    public string IntersectionEntryPoint { get; set; }
}
