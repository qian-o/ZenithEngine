﻿using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKGraphicsPipeline : GraphicsPipeline
{
    public VkPipelineLayout PipelineLayout;
    public VkPipeline Pipeline;

    public VKGraphicsPipeline(GraphicsContext context,
                              ref readonly GraphicsPipelineDesc desc) : base(context, in desc)
    {
        GraphicsPipelineCreateInfo createInfo = new()
        {
            SType = StructureType.GraphicsPipelineCreateInfo
        };

        // Render States
        {
            uint attachmentCount = (uint)desc.Outputs.ColorAttachments.Length;

            PipelineRasterizationStateCreateInfo rasterizationState = new()
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                DepthClampEnable = desc.RenderStates.RasterizerState.DepthClipEnabled,
                PolygonMode = VKFormats.GetPolygonMode(desc.RenderStates.RasterizerState.FillMode),
                CullMode = VKFormats.GetCullModeFlags(desc.RenderStates.RasterizerState.CullMode),
                FrontFace = VKFormats.GetFrontFace(desc.RenderStates.RasterizerState.FrontFace),
                DepthBiasEnable = true,
                DepthBiasConstantFactor = desc.RenderStates.RasterizerState.DepthBias,
                DepthBiasClamp = desc.RenderStates.RasterizerState.DepthBiasClamp,
                DepthBiasSlopeFactor = desc.RenderStates.RasterizerState.SlopeScaledDepthBias,
                LineWidth = 1
            };

            createInfo.PRasterizationState = &rasterizationState;

            PipelineDepthStencilStateCreateInfo depthStencilState = new()
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = desc.RenderStates.DepthStencilState.DepthEnabled,
                DepthWriteEnable = desc.RenderStates.DepthStencilState.DepthWriteEnabled,
                DepthCompareOp = VKFormats.GetCompareOp(desc.RenderStates.DepthStencilState.DepthFunction),
                StencilTestEnable = desc.RenderStates.DepthStencilState.StencilEnabled,
                Front = new()
                {
                    FailOp = VKFormats.GetStencilOp(desc.RenderStates.DepthStencilState.FrontFace.StencilFailOperation),
                    PassOp = VKFormats.GetStencilOp(desc.RenderStates.DepthStencilState.FrontFace.StencilPassOperation),
                    DepthFailOp = VKFormats.GetStencilOp(desc.RenderStates.DepthStencilState.FrontFace.StencilDepthFailOperation),
                    CompareOp = VKFormats.GetCompareOp(desc.RenderStates.DepthStencilState.FrontFace.StencilFunction),
                    CompareMask = desc.RenderStates.DepthStencilState.StencilReadMask,
                    WriteMask = desc.RenderStates.DepthStencilState.StencilWriteMask,
                    Reference = (uint)desc.RenderStates.StencilReference
                },
                Back = new()
                {
                    FailOp = VKFormats.GetStencilOp(desc.RenderStates.DepthStencilState.BackFace.StencilFailOperation),
                    PassOp = VKFormats.GetStencilOp(desc.RenderStates.DepthStencilState.BackFace.StencilPassOperation),
                    DepthFailOp = VKFormats.GetStencilOp(desc.RenderStates.DepthStencilState.BackFace.StencilDepthFailOperation),
                    CompareOp = VKFormats.GetCompareOp(desc.RenderStates.DepthStencilState.BackFace.StencilFunction),
                    CompareMask = desc.RenderStates.DepthStencilState.StencilReadMask,
                    WriteMask = desc.RenderStates.DepthStencilState.StencilWriteMask,
                    Reference = (uint)desc.RenderStates.StencilReference
                },
                MinDepthBounds = 0,
                MaxDepthBounds = 1
            };

            createInfo.PDepthStencilState = &depthStencilState;

            PipelineColorBlendStateCreateInfo colorBlendState = new()
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo
            };

            BlendStateRenderTargetDesc[] renderTargets =
            [
                desc.RenderStates.BlendState.RenderTarget0,
                desc.RenderStates.BlendState.RenderTarget1,
                desc.RenderStates.BlendState.RenderTarget2,
                desc.RenderStates.BlendState.RenderTarget3,
                desc.RenderStates.BlendState.RenderTarget4,
                desc.RenderStates.BlendState.RenderTarget5,
                desc.RenderStates.BlendState.RenderTarget6,
                desc.RenderStates.BlendState.RenderTarget7
            ];

            PipelineColorBlendAttachmentState* attachments = Allocator.Alloc<PipelineColorBlendAttachmentState>(attachmentCount);

            for (uint i = 0; i < attachmentCount; i++)
            {
                BlendStateRenderTargetDesc renderTarget = desc.RenderStates.BlendState.IndependentBlendEnabled ? renderTargets[i] : renderTargets[0];

                attachments[i] = new()
                {
                    BlendEnable = renderTarget.BlendEnabled,
                    SrcColorBlendFactor = VKFormats.GetBlendFactor(renderTarget.SourceBlendColor),
                    DstColorBlendFactor = VKFormats.GetBlendFactor(renderTarget.DestinationBlendColor),
                    ColorBlendOp = VKFormats.GetBlendOp(renderTarget.BlendOperationColor),
                    SrcAlphaBlendFactor = VKFormats.GetBlendFactor(renderTarget.SourceBlendAlpha),
                    DstAlphaBlendFactor = VKFormats.GetBlendFactor(renderTarget.DestinationBlendAlpha),
                    AlphaBlendOp = VKFormats.GetBlendOp(renderTarget.BlendOperationAlpha),
                    ColorWriteMask = VKFormats.GetColorComponentFlags(renderTarget.ColorWriteChannels)
                };
            }

            colorBlendState.AttachmentCount = attachmentCount;
            colorBlendState.PAttachments = attachments;

            if (desc.RenderStates.BlendFactor.HasValue)
            {
                colorBlendState.BlendConstants[0] = desc.RenderStates.BlendFactor.Value.X;
                colorBlendState.BlendConstants[1] = desc.RenderStates.BlendFactor.Value.Y;
                colorBlendState.BlendConstants[2] = desc.RenderStates.BlendFactor.Value.Z;
                colorBlendState.BlendConstants[3] = desc.RenderStates.BlendFactor.Value.W;
            }

            createInfo.PColorBlendState = &colorBlendState;
        }

        // Shaders
        {
            List<PipelineShaderStageCreateInfo> shaderStages = [];

            if (desc.Shaders.Vertex is not null)
            {
                shaderStages.Add(desc.Shaders.Vertex.VK().PipelineShaderStageCreateInfo);
            }

            if (desc.Shaders.Hull is not null)
            {
                shaderStages.Add(desc.Shaders.Hull.VK().PipelineShaderStageCreateInfo);
            }

            if (desc.Shaders.Domain is not null)
            {
                shaderStages.Add(desc.Shaders.Domain.VK().PipelineShaderStageCreateInfo);
            }

            if (desc.Shaders.Geometry is not null)
            {
                shaderStages.Add(desc.Shaders.Geometry.VK().PipelineShaderStageCreateInfo);
            }

            if (desc.Shaders.Pixel is not null)
            {
                shaderStages.Add(desc.Shaders.Pixel.VK().PipelineShaderStageCreateInfo);
            }

            PipelineShaderStageCreateInfo* stages = Allocator.Alloc([.. shaderStages]);

            createInfo.StageCount = (uint)shaderStages.Count;
            createInfo.PStages = stages;
        }

        // Input Layouts
        {
            uint vertexInputBindingCount = (uint)desc.InputLayouts.Length;
            uint vertexInputAttributeCount = (uint)desc.InputLayouts.Sum(static item => item.Elements.Length);

            VertexInputBindingDescription* bindingDescriptions = Allocator.Alloc<VertexInputBindingDescription>(vertexInputBindingCount);
            VertexInputAttributeDescription* attributeDescriptions = Allocator.Alloc<VertexInputAttributeDescription>(vertexInputAttributeCount);

            uint bindingLocation = 0;
            uint attributeIndex = 0;
            for (uint i = 0; i < vertexInputBindingCount; i++)
            {
                InputLayoutDesc inputLayout = desc.InputLayouts[i];

                bindingDescriptions[i] = new()
                {
                    Binding = i,
                    Stride = inputLayout.Stride,
                    InputRate = inputLayout.StepRate is 0 ? VertexInputRate.Vertex : VertexInputRate.Instance
                };

                for (int j = 0; j < inputLayout.Elements.Length; j++)
                {
                    InputElementDesc element = inputLayout.Elements[j];

                    attributeDescriptions[attributeIndex] = new()
                    {
                        Binding = i,
                        Location = (uint)(bindingLocation + j),
                        Format = VKFormats.GetElementFormat(element.Format),
                        Offset = (uint)element.Offset
                    };

                    attributeIndex++;
                }

                bindingLocation += (uint)inputLayout.Elements.Length;
            }

            PipelineVertexInputStateCreateInfo vertexInputState = new()
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = vertexInputBindingCount,
                PVertexBindingDescriptions = bindingDescriptions,
                VertexAttributeDescriptionCount = vertexInputAttributeCount,
                PVertexAttributeDescriptions = attributeDescriptions
            };

            createInfo.PVertexInputState = &vertexInputState;
        }

        // Resource Layouts
        {
            DescriptorSetLayout* setLayouts = Allocator.Alloc([.. desc.ResourceLayouts.Select(static item => item.VK().DescriptorSetLayout)]);

            PipelineLayoutCreateInfo pipelineLayoutCreateInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = (uint)desc.ResourceLayouts.Length,
                PSetLayouts = setLayouts
            };

            Context.Vk.CreatePipelineLayout(Context.Device,
                                            &pipelineLayoutCreateInfo,
                                            null,
                                            out PipelineLayout).ThrowIfError();

            createInfo.Layout = PipelineLayout;
        }

        // Primitive Topology
        {
            PipelineInputAssemblyStateCreateInfo inputAssemblyState = new()
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = VKFormats.GetPrimitiveTopology(desc.PrimitiveTopology)
            };

            if (desc.PrimitiveTopology >= PrimitiveTopology.PatchList)
            {
                uint patchControlPoints = (uint)(desc.PrimitiveTopology - PrimitiveTopology.PatchList + 1);

                PipelineTessellationStateCreateInfo tessellationState = new()
                {
                    SType = StructureType.PipelineTessellationStateCreateInfo,
                    PatchControlPoints = patchControlPoints
                };

                createInfo.PTessellationState = &tessellationState;
            }

            createInfo.PInputAssemblyState = &inputAssemblyState;
        }

        // Outputs
        {
            uint colorAttachmentCount = (uint)desc.Outputs.ColorAttachments.Length;
            Format* colorAttachmentFormats = Allocator.Alloc([.. desc.Outputs.ColorAttachments.Select(VKFormats.GetFormat)]);

            PipelineViewportStateCreateInfo viewportState = new()
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = colorAttachmentCount,
                ScissorCount = colorAttachmentCount
            };

            createInfo.PViewportState = &viewportState;

            PipelineMultisampleStateCreateInfo multisampleState = new()
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                RasterizationSamples = VKFormats.GetSampleCountFlags(desc.Outputs.SampleCount)
            };

            createInfo.PMultisampleState = &multisampleState;

            PipelineRenderingCreateInfo renderingCreateInfo = new()
            {
                SType = StructureType.PipelineRenderingCreateInfo,
                ColorAttachmentCount = colorAttachmentCount,
                PColorAttachmentFormats = colorAttachmentFormats
            };

            if (desc.Outputs.DepthStencilAttachment.HasValue)
            {
                Format depthStencilAttachmentFormat = VKFormats.GetFormat(desc.Outputs.DepthStencilAttachment.Value);

                renderingCreateInfo.DepthAttachmentFormat = depthStencilAttachmentFormat;
                renderingCreateInfo.StencilAttachmentFormat = depthStencilAttachmentFormat;
            }

            createInfo.PNext = &renderingCreateInfo;
        }

        // Other pipeline states
        {
            DynamicState* dynamicStates = Allocator.Alloc([DynamicState.Viewport, DynamicState.Scissor]);

            PipelineDynamicStateCreateInfo dynamicState = new()
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 2,
                PDynamicStates = dynamicStates
            };

            createInfo.PDynamicState = &dynamicState;
        }

        Context.Vk.CreateGraphicsPipelines(Context.Device,
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
