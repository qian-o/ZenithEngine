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

            string[] entryPoints = [.. shaders.Select(static item => item.Desc.EntryPoint)];

            PipelineShaderStageCreateInfo* stages = Allocator.Alloc([.. shaders.Select(static item => item.VK().PipelineShaderStageCreateInfo)]);

            createInfo.StageCount = (uint)shaders.Length;
            createInfo.PStages = stages;

            uint index = 0;
            uint groupCount = (uint)(1 + desc.Shaders.Miss.Length + desc.HitGroups.Length);
            RayTracingShaderGroupCreateInfoKHR* groups = Allocator.Alloc<RayTracingShaderGroupCreateInfoKHR>(groupCount);

            groups[index++] = new()
            {
                SType = StructureType.RayTracingShaderGroupCreateInfoKhr,
                Type = RayTracingShaderGroupTypeKHR.GeneralKhr,
                GeneralShader = (uint)Array.IndexOf(entryPoints, desc.Shaders.RayGen.Desc.EntryPoint),
                ClosestHitShader = Vk.ShaderUnusedKhr,
                AnyHitShader = Vk.ShaderUnusedKhr,
                IntersectionShader = Vk.ShaderUnusedKhr
            };

            for (int i = 0; i < desc.Shaders.Miss.Length; i++)
            {
                groups[index++] = new()
                {
                    SType = StructureType.RayTracingShaderGroupCreateInfoKhr,
                    Type = RayTracingShaderGroupTypeKHR.GeneralKhr,
                    GeneralShader = (uint)Array.IndexOf(entryPoints, desc.Shaders.Miss[i].Desc.EntryPoint),
                    ClosestHitShader = Vk.ShaderUnusedKhr,
                    AnyHitShader = Vk.ShaderUnusedKhr,
                    IntersectionShader = Vk.ShaderUnusedKhr
                };
            }

            for (int i = 0; i < desc.HitGroups.Length; i++)
            {
                HitGroupDesc hitGroup = desc.HitGroups[i];

                uint closestHit = hitGroup.ClosestHit is not null ? (uint)Array.IndexOf(entryPoints, hitGroup.ClosestHit) : Vk.ShaderUnusedKhr;
                uint anyHit = hitGroup.AnyHit is not null ? (uint)Array.IndexOf(entryPoints, hitGroup.AnyHit) : Vk.ShaderUnusedKhr;
                uint intersection = hitGroup.Intersection is not null ? (uint)Array.IndexOf(entryPoints, hitGroup.Intersection) : Vk.ShaderUnusedKhr;

                groups[index++] = new()
                {
                    SType = StructureType.RayTracingShaderGroupCreateInfoKhr,
                    Type = VKFormats.GetRayTracingShaderGroupType(hitGroup.Type),
                    GeneralShader = Vk.ShaderUnusedKhr,
                    ClosestHitShader = closestHit,
                    AnyHitShader = anyHit,
                    IntersectionShader = intersection
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

        ShaderTable = new(Context, Pipeline, 1, (uint)desc.Shaders.Miss.Length, (uint)desc.HitGroups.Length);

        Allocator.Release();
    }

    public VKShaderTable ShaderTable { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        DebugUtilsObjectNameInfoEXT nameInfo = new()
        {
            SType = StructureType.DebugUtilsObjectNameInfoExt,
            ObjectType = ObjectType.Pipeline,
            ObjectHandle = Pipeline.Handle,
            PObjectName = Allocator.AllocUTF8(name)
        };

        Context.ExtDebugUtils!.SetDebugUtilsObjectName(Context.Device, &nameInfo).ThrowIfError();
    }

    protected override void Destroy()
    {
        ShaderTable.Dispose();

        Context.Vk.DestroyPipeline(Context.Device, Pipeline, null);
        Context.Vk.DestroyPipelineLayout(Context.Device, PipelineLayout, null);
    }
}
