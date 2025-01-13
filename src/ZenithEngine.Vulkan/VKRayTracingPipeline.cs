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


        // Shaders and Hit Groups
        {
            Shader[] shaders =
            [
                desc.Shaders.RayGen,
                .. desc.Shaders.Miss,
                .. desc.Shaders.ClosestHit,
                .. desc.Shaders.AnyHit,
                .. desc.Shaders.Intersection
            ];

            PipelineShaderStageCreateInfo* stages = Allocator.Alloc([.. shaders.Select(static item => item.VK().PipelineShaderStageCreateInfo)]);

            createInfo.StageCount = (uint)shaders.Length;
            createInfo.PStages = stages;

            uint groupCount = (uint)desc.HitGroups.Length;
            string[] entryPoints = [.. shaders.Select(static item => item.Desc.EntryPoint)];
            RayTracingShaderGroupCreateInfoKHR* groups = Allocator.Alloc<RayTracingShaderGroupCreateInfoKHR>(groupCount);

            for (uint i = 0; i < groupCount; i++)
            {
                HitGroupDesc hitGroup = desc.HitGroups[i];

                uint closestHitIndex = hitGroup.ClosestHit is not null ? (uint)Array.IndexOf(entryPoints, hitGroup.ClosestHit) : Vk.ShaderUnusedKhr;
                uint anyHitIndex = hitGroup.AnyHit is not null ? (uint)Array.IndexOf(entryPoints, hitGroup.AnyHit) : Vk.ShaderUnusedKhr;
                uint intersectionIndex = hitGroup.Intersection is not null ? (uint)Array.IndexOf(entryPoints, hitGroup.Intersection) : Vk.ShaderUnusedKhr;

                groups[i] = new()
                {
                    SType = StructureType.RayTracingShaderGroupCreateInfoKhr,
                    Type = VKFormats.GetRayTracingShaderGroupType(hitGroup.Type),
                    GeneralShader = Vk.ShaderUnusedKhr,
                    ClosestHitShader = closestHitIndex,
                    AnyHitShader = anyHitIndex,
                    IntersectionShader = intersectionIndex
                };
            }

            createInfo.GroupCount = groupCount;
            createInfo.PGroups = groups;
        }

        // Resource Layouts
        {
            PipelineLayoutCreateInfo pipelineLayoutCreateInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = (uint)desc.ResourceLayouts.Length,
                PSetLayouts = Allocator.Alloc([.. desc.ResourceLayouts.Select(static item => item.VK().DescriptorSetLayout)])
            };

            Context.Vk.CreatePipelineLayout(Context.Device,
                                            &pipelineLayoutCreateInfo,
                                            null,
                                            out PipelineLayout).ThrowIfError();

            createInfo.Layout = PipelineLayout;
        }

        Context.KhrRayTracingPipeline!.CreateRayTracingPipelines(Context.Device,
                                                                 default,
                                                                 default,
                                                                 1,
                                                                 &createInfo,
                                                                 null,
                                                                 out Pipeline).ThrowIfError();

        Allocator.Release();
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
