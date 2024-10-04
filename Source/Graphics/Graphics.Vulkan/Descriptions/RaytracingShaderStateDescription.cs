namespace Graphics.Vulkan.Descriptions;

public record struct RaytracingShaderStateDescription
{
    public RaytracingShaderStateDescription(Shader rayGenerationShader,
                                            Shader[] missShader,
                                            Shader[] closestHitShader,
                                            Shader[] anyHitShader,
                                            Shader[] intersectionShader)
    {
        RayGenerationShader = rayGenerationShader;
        MissShader = missShader;
        ClosestHitShader = closestHitShader;
        AnyHitShader = anyHitShader;
        IntersectionShader = intersectionShader;
    }

    /// <summary>
    /// Gets or sets the Raygeneration shader program.
    /// </summary>
    public Shader RayGenerationShader { get; set; }

    /// <summary>
    /// Gets or sets the Miss shader program.
    /// </summary>
    public Shader[] MissShader { get; set; }

    /// <summary>
    /// Gets or sets the closestHit shader program.
    /// </summary>
    public Shader[] ClosestHitShader { get; set; }

    /// <summary>
    /// Gets or sets the AnyHit shader program.
    /// </summary>
    public Shader[] AnyHitShader { get; set; }

    /// <summary>
    /// Gets or sets the Intersection shader program.
    /// </summary>
    public Shader[] IntersectionShader { get; set; }
}
