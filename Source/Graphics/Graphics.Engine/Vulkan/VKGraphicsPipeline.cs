using Graphics.Core.Helpers;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKGraphicsPipeline : GraphicsPipeline
{
    public VKGraphicsPipeline(Context context,
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
                CullMode = Formats.GetCullModeFlags(desc.RenderStates.RasterizerState.CullMode),
                PolygonMode = Formats.GetPolygonMode(desc.RenderStates.RasterizerState.FillMode),
                FrontFace = Formats.GetFrontFace(desc.RenderStates.RasterizerState.FrontFace),
                DepthBiasEnable = true,
                DepthBiasConstantFactor = desc.RenderStates.RasterizerState.DepthBias,
                DepthBiasClamp = desc.RenderStates.RasterizerState.DepthBiasClamp,
                DepthBiasSlopeFactor = desc.RenderStates.RasterizerState.SlopeScaledDepthBias,
                DepthClampEnable = desc.RenderStates.RasterizerState.DepthClipEnabled
            };

            createInfo.PRasterizationState = &rasterizationState;

            PipelineDepthStencilStateCreateInfo depthStencilState = new()
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = desc.RenderStates.DepthStencilState.DepthEnabled,
                DepthWriteEnable = desc.RenderStates.DepthStencilState.DepthWriteEnabled,
                DepthCompareOp = Formats.GetCompareOp(desc.RenderStates.DepthStencilState.DepthFunction),
                StencilTestEnable = desc.RenderStates.DepthStencilState.StencilEnabled,
                Front = new()
                {
                    FailOp = Formats.GetStencilOp(desc.RenderStates.DepthStencilState.FrontFace.StencilFailOperation),
                    DepthFailOp = Formats.GetStencilOp(desc.RenderStates.DepthStencilState.FrontFace.StencilDepthFailOperation),
                    PassOp = Formats.GetStencilOp(desc.RenderStates.DepthStencilState.FrontFace.StencilPassOperation),
                    CompareOp = Formats.GetCompareOp(desc.RenderStates.DepthStencilState.FrontFace.StencilFunction),
                    CompareMask = desc.RenderStates.DepthStencilState.StencilReadMask,
                    WriteMask = desc.RenderStates.DepthStencilState.StencilWriteMask,
                    Reference = (uint)desc.RenderStates.StencilReference
                },
                Back = new()
                {
                    FailOp = Formats.GetStencilOp(desc.RenderStates.DepthStencilState.BackFace.StencilFailOperation),
                    DepthFailOp = Formats.GetStencilOp(desc.RenderStates.DepthStencilState.BackFace.StencilDepthFailOperation),
                    PassOp = Formats.GetStencilOp(desc.RenderStates.DepthStencilState.BackFace.StencilPassOperation),
                    CompareOp = Formats.GetCompareOp(desc.RenderStates.DepthStencilState.BackFace.StencilFunction),
                    CompareMask = desc.RenderStates.DepthStencilState.StencilReadMask,
                    WriteMask = desc.RenderStates.DepthStencilState.StencilWriteMask,
                    Reference = (uint)desc.RenderStates.StencilReference
                }
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

            PipelineColorBlendAttachmentState[] attachments = new PipelineColorBlendAttachmentState[attachmentCount];

            for (uint i = 0; i < attachmentCount; i++)
            {
                BlendStateRenderTargetDesc renderTarget = desc.RenderStates.BlendState.IndependentBlendEnabled ? renderTargets[i] : renderTargets[0];

                attachments[i] = new PipelineColorBlendAttachmentState
                {
                    BlendEnable = renderTarget.BlendEnabled,
                    SrcColorBlendFactor = Formats.GetBlendFactor(renderTargets[i].SourceBlendColor),
                    DstColorBlendFactor = Formats.GetBlendFactor(renderTargets[i].DestinationBlendColor),
                    ColorBlendOp = Formats.GetBlendOp(renderTargets[i].BlendOperationColor),
                    SrcAlphaBlendFactor = Formats.GetBlendFactor(renderTargets[i].SourceBlendAlpha),
                    DstAlphaBlendFactor = Formats.GetBlendFactor(renderTargets[i].DestinationBlendAlpha),
                    AlphaBlendOp = Formats.GetBlendOp(renderTargets[i].BlendOperationAlpha),
                    ColorWriteMask = Formats.GetColorComponentFlags(renderTargets[i].ColorWriteChannels)
                };
            }

            colorBlendState.AttachmentCount = attachmentCount;
            colorBlendState.PAttachments = attachments.AsPointer();

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

        }

        // Outputs
        {
            PipelineMultisampleStateCreateInfo multisampleState = new()
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                RasterizationSamples = Formats.GetSampleCountFlags(desc.Outputs.SampleCount)
            };

            createInfo.PMultisampleState = &multisampleState;
        }
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkPipeline Pipeline { get; }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.Pipeline, Pipeline.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyPipeline(Context.Device, Pipeline, null);
    }
}
