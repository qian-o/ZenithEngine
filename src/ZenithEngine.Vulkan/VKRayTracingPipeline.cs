using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKRayTracingPipeline : RayTracingPipeline
{
    public VkPipelineLayout PipelineLayout;
    public VkPipeline Pipeline;

    public VKRayTracingPipeline(GraphicsContext context,
                                ref readonly RayTracingPipelineDesc desc) : base(context, in desc)
    {
        RayTracingPipelineCreateInfoKHR createInfo = new()
        {
            SType = StructureType.RayTracingPipelineCreateInfoKhr,
            MaxPipelineRayRecursionDepth = desc.MaxTraceRecursionDepth
        };

        // Shaders
        {
        }
    }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.Pipeline, Pipeline.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyPipeline(Context.Device, Pipeline, null);
        Context.Vk.DestroyPipelineLayout(Context.Device, PipelineLayout, null);
    }
}
