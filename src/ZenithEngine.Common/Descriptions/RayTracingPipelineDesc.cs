using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct RayTracingPipelineDesc(RayTracingShaderDesc shaders,
                                     HitGroupDesc[] hitGroups,
                                     ResourceLayout[] resourceLayouts,
                                     uint maxTraceRecursionDepth = 6,
                                     uint maxPayloadSizeInBytes = 256,
                                     uint maxAttributeSizeInBytes = 32)
{
    public RayTracingPipelineDesc() : this(new(), [], [], 6, 256, 32)
    {
    }

    /// <summary>
    /// The ray tracing shader description.
    /// </summary>
    public RayTracingShaderDesc Shaders = shaders;

    /// <summary>
    /// Describes the hit groups input array.
    /// </summary>
    public HitGroupDesc[] HitGroups = hitGroups;

    /// <summary>
    /// Describes the resource layouts input array.
    /// </summary>
    public ResourceLayout[] ResourceLayouts = resourceLayouts;

    /// <summary>
    /// Limit on ray recursion for the raytracing pipeline. It must be in the range of
    /// 0 to 31. Below the maximum recursion depth, shader invocations such as closest
    /// depth, TraceRay calls result in the device going into removed state.
    /// </summary>
    public uint MaxTraceRecursionDepth = maxTraceRecursionDepth;

    /// <summary>
    /// The maximum storage for scalars (counted as 4 bytes each) in ray payloads in
    /// raytracing pipelines that contain this program.
    /// </summary>
    public uint MaxPayloadSizeInBytes = maxPayloadSizeInBytes;

    /// <summary>
    /// The maximum number of scalars (counted as 4 bytes each) that can be used for
    /// attributes in pipelines that contain this shader.
    /// </summary>
    public uint MaxAttributeSizeInBytes = maxAttributeSizeInBytes;
}
