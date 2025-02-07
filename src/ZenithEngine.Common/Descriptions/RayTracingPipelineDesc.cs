using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct RayTracingPipelineDesc(RayTracingShaderDesc shaders,
                                     HitGroupDesc[] hitGroups,
                                     ResourceLayout[] resourceLayouts,
                                     uint maxTraceRecursionDepth = 6)
{
    public RayTracingPipelineDesc()
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
}
