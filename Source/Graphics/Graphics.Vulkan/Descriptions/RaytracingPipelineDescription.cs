namespace Graphics.Vulkan.Descriptions;

public record struct RaytracingPipelineDescription
{
    public RaytracingPipelineDescription(RaytracingShaderStateDescription shaders,
                                         HitGroupDescription[] hitGroups,
                                         ResourceLayout[] resourceLayouts,
                                         uint maxTraceRecursionDepth,
                                         uint maxPayloadSizeInBytes,
                                         uint maxAttributeSizeInBytes)
    {
        Shaders = shaders;
        HitGroups = hitGroups;
        ResourceLayouts = resourceLayouts;
        MaxTraceRecursionDepth = maxTraceRecursionDepth;
        MaxPayloadSizeInBytes = maxPayloadSizeInBytes;
        MaxAttributeSizeInBytes = maxAttributeSizeInBytes;
    }

    /// <summary>
    ///  Gets or sets the raytracing shader program.
    /// </summary>
    public RaytracingShaderStateDescription Shaders { get; set; }

    /// <summary>
    /// Gets or sets the raytracing hit groups.
    /// </summary>
    public HitGroupDescription[] HitGroups { get; set; }

    /// <summary>
    /// Describes the resources layout input.
    /// </summary>
    public ResourceLayout[] ResourceLayouts { get; set; }

    /// <summary>
    /// Limit on ray recursion for the raytracing pipeline. It must be in the range of
    /// 0 to 31. Below the maximum recursion depth, shader invocations such as closest
    /// depth, TraceRay calls result in the device going into removed state.
    /// </summary>
    public uint MaxTraceRecursionDepth { get; set; }

    /// <summary>
    /// The maximum storage for scalars (counted as 4 bytes each) in ray payloads in
    /// raytracing pipelines that contain this program. Callable shader payloads are
    /// not part of this limit. This field is ignored for payloads that use payload access
    /// qualifiers.
    /// </summary>
    public uint MaxPayloadSizeInBytes { get; set; }

    /// <summary>
    /// The maximum number of scalars (counted as 4 bytes each) that can be used for
    /// attributes in pipelines that contain this shader. The value cannot exceed D3D12_RAYTRACING_MAX_ATTRIBUTE_SIZE_IN_BYTES
    /// constant (https://microsoft.github.io/DirectX-Specs/d3d/Raytracing.html#constants).
    /// </summary>
    public uint MaxAttributeSizeInBytes { get; set; }
}
