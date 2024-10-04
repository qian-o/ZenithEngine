namespace Graphics.Vulkan.Descriptions;

public record struct RaytracingShaderStateDescription
{
    /// <summary>
    /// Gets or sets the Raygeneration shader program.
    /// </summary>
    public Shader RayGenerationShader { get; set; }

    /// <summary>
    /// Gets or sets the closestHit shader program.
    /// </summary>
    public Shader[] ClosestHitShader { get; set; }

    /// <summary>
    /// Gets or sets the Miss shader program.
    /// </summary>
    public Shader[] MissShader { get; set; }

    /// <summary>
    /// Gets or sets the AnyHit shader program.
    /// </summary>
    public Shader[] AnyHitShader { get; set; }

    /// <summary>
    /// Gets or sets the Intersection shader program.
    /// </summary>
    public Shader[] IntersectionShader { get; set; }
}
