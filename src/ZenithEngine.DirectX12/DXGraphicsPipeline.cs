﻿using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXGraphicsPipeline : GraphicsPipeline
{
    public ComPtr<ID3D12RootSignature> RootSignature;
    public ComPtr<ID3D12PipelineState> PipelineState;

    public DXGraphicsPipeline(GraphicsContext context,
                              ref readonly GraphicsPipelineDesc desc) : base(context, in desc)
    {
        GraphicsPipelineStateDesc graphicsPipelineStateDesc = new()
        {
            SampleMask = uint.MaxValue,
        };

        // Render States
        {
            graphicsPipelineStateDesc.RasterizerState = new()
            {
                FillMode = DXFormats.GetFillMode(desc.RenderStates.RasterizerState.FillMode),
                CullMode = DXFormats.GetCullMode(desc.RenderStates.RasterizerState.CullMode),
                FrontCounterClockwise = desc.RenderStates.RasterizerState.FrontFace is FrontFace.CounterClockwise,
                DepthBias = desc.RenderStates.RasterizerState.DepthBias,
                DepthBiasClamp = desc.RenderStates.RasterizerState.DepthBiasClamp,
                SlopeScaledDepthBias = desc.RenderStates.RasterizerState.SlopeScaledDepthBias,
                DepthClipEnable = desc.RenderStates.RasterizerState.DepthClipEnabled,
                MultisampleEnable = desc.Outputs.SampleCount is not TextureSampleCount.Count1,
                AntialiasedLineEnable = false,
                ForcedSampleCount = 0,
                ConservativeRaster = ConservativeRasterizationMode.Off
            };

            graphicsPipelineStateDesc.DepthStencilState = new()
            {
                DepthEnable = desc.RenderStates.DepthStencilState.DepthEnabled,
                DepthWriteMask = desc.RenderStates.DepthStencilState.DepthWriteEnabled ? DepthWriteMask.All : DepthWriteMask.Zero,
                DepthFunc = DXFormats.GetComparisonFunc(desc.RenderStates.DepthStencilState.DepthFunction),
                StencilEnable = desc.RenderStates.DepthStencilState.StencilEnabled,
                StencilReadMask = desc.RenderStates.DepthStencilState.StencilReadMask,
                StencilWriteMask = desc.RenderStates.DepthStencilState.StencilWriteMask,
                FrontFace = new()
                {
                    StencilFailOp = DXFormats.GetStencilOp(desc.RenderStates.DepthStencilState.FrontFace.StencilFailOperation),
                    StencilDepthFailOp = DXFormats.GetStencilOp(desc.RenderStates.DepthStencilState.FrontFace.StencilDepthFailOperation),
                    StencilPassOp = DXFormats.GetStencilOp(desc.RenderStates.DepthStencilState.FrontFace.StencilPassOperation),
                    StencilFunc = DXFormats.GetComparisonFunc(desc.RenderStates.DepthStencilState.FrontFace.StencilFunction)
                },
                BackFace = new()
                {
                    StencilFailOp = DXFormats.GetStencilOp(desc.RenderStates.DepthStencilState.BackFace.StencilFailOperation),
                    StencilDepthFailOp = DXFormats.GetStencilOp(desc.RenderStates.DepthStencilState.BackFace.StencilDepthFailOperation),
                    StencilPassOp = DXFormats.GetStencilOp(desc.RenderStates.DepthStencilState.BackFace.StencilPassOperation),
                    StencilFunc = DXFormats.GetComparisonFunc(desc.RenderStates.DepthStencilState.BackFace.StencilFunction)
                }
            };

            graphicsPipelineStateDesc.BlendState = new()
            {
                AlphaToCoverageEnable = desc.RenderStates.BlendState.AlphaToCoverageEnabled,
                IndependentBlendEnable = desc.RenderStates.BlendState.IndependentBlendEnabled
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

            Span<RenderTargetBlendDesc> renderTargetDescs = graphicsPipelineStateDesc.BlendState.RenderTarget.AsSpan();

            for (int i = 0; i < renderTargets.Length; i++)
            {
                BlendStateRenderTargetDesc renderTarget = desc.RenderStates.BlendState.IndependentBlendEnabled ? renderTargets[i] : renderTargets[0];

                renderTargetDescs[i] = new()
                {
                    BlendEnable = renderTarget.BlendEnabled,
                    SrcBlend = DXFormats.GetBlend(renderTarget.SourceBlendColor),
                    DestBlend = DXFormats.GetBlend(renderTarget.DestinationBlendColor),
                    BlendOp = DXFormats.GetBlendOp(renderTarget.BlendOperationColor),
                    SrcBlendAlpha = DXFormats.GetBlend(renderTarget.SourceBlendAlpha),
                    DestBlendAlpha = DXFormats.GetBlend(renderTarget.DestinationBlendAlpha),
                    BlendOpAlpha = DXFormats.GetBlendOp(renderTarget.BlendOperationAlpha),
                    RenderTargetWriteMask = (byte)DXFormats.GetColorWriteEnable(renderTarget.ColorWriteChannels)
                };
            }
        }

        // Shaders
        {
            if (desc.Shaders.Vertex is not null)
            {
                graphicsPipelineStateDesc.VS = desc.Shaders.Vertex.DX().Shader;
            }

            if (desc.Shaders.Hull is not null)
            {
                graphicsPipelineStateDesc.PS = desc.Shaders.Hull.DX().Shader;
            }

            if (desc.Shaders.Domain is not null)
            {
                graphicsPipelineStateDesc.DS = desc.Shaders.Domain.DX().Shader;
            }

            if (desc.Shaders.Geometry is not null)
            {
                graphicsPipelineStateDesc.GS = desc.Shaders.Geometry.DX().Shader;
            }

            if (desc.Shaders.Pixel is not null)
            {
                graphicsPipelineStateDesc.PS = desc.Shaders.Pixel.DX().Shader;
            }
        }

        // Input Layouts
        {
            uint numElements = (uint)desc.InputLayouts.Sum(x => x.Elements.Length);
            InputElementDesc* pInputElementDescs = Allocator.Alloc<InputElementDesc>(numElements);

            uint offset = 0;
            for (int i = 0; i < desc.InputLayouts.Length; i++)
            {
                LayoutDesc layoutDesc = desc.InputLayouts[i];

                foreach (ElementDesc elementDesc in layoutDesc.Elements)
                {
                    pInputElementDescs[offset++] = new()
                    {
                        SemanticName = Allocator.AllocUTF8(elementDesc.Semantic.ToString().ToUpper()),
                        SemanticIndex = elementDesc.SemanticIndex,
                        Format = DXFormats.GetFormat(elementDesc.Format),
                        InputSlot = (uint)i,
                        AlignedByteOffset = (uint)elementDesc.Offset,
                        InputSlotClass = DXFormats.GetInputClassification(layoutDesc.StepFunction),
                        InstanceDataStepRate = layoutDesc.StepRate
                    };
                }
            }

            graphicsPipelineStateDesc.InputLayout = new()
            {
                NumElements = numElements,
                PInputElementDescs = pInputElementDescs
            };
        }
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
