﻿using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKComputePipeline : ComputePipeline
{
    public VkPipelineLayout PipelineLayout;
    public VkPipeline Pipeline;

    public VKComputePipeline(GraphicsContext context,
                             ref readonly ComputePipelineDesc desc) : base(context, in desc)
    {
        ComputePipelineCreateInfo createInfo = new()
        {
            SType = StructureType.ComputePipelineCreateInfo
        };

        // Shader
        {
            createInfo.Stage = desc.Shader.VK().PipelineShaderStageCreateInfo;
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

        Context.Vk.CreateComputePipelines(Context.Device,
                                          default,
                                          1,
                                          &createInfo,
                                          null,
                                          out Pipeline).ThrowIfError();

        Allocator.Release();
    }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void SetName(string name)
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
        Context.Vk.DestroyPipeline(Context.Device, Pipeline, null);
        Context.Vk.DestroyPipelineLayout(Context.Device, PipelineLayout, null);
    }
}
