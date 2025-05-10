using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
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
            SampleMask = uint.MaxValue
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
            uint numElements = (uint)desc.InputLayouts.Sum(static item => item.Elements.Length);
            DxInputElementDesc* pInputElementDescs = Allocator.Alloc<DxInputElementDesc>(numElements);

            uint offset = 0;
            for (int i = 0; i < desc.InputLayouts.Length; i++)
            {
                InputLayoutDesc inputLayout = desc.InputLayouts[i];

                foreach (InputElementDesc element in inputLayout.Elements)
                {
                    pInputElementDescs[offset++] = new()
                    {
                        SemanticName = Allocator.AllocUTF8(element.Semantic.ToString().ToUpper()),
                        SemanticIndex = element.SemanticIndex,
                        Format = DXFormats.GetFormat(element.Format),
                        InputSlot = (uint)i,
                        AlignedByteOffset = (uint)element.Offset,
                        InputSlotClass = DXFormats.GetInputClassification(inputLayout.StepFunction),
                        InstanceDataStepRate = inputLayout.StepRate
                    };
                }
            }

            graphicsPipelineStateDesc.InputLayout = new()
            {
                NumElements = numElements,
                PInputElementDescs = pInputElementDescs
            };
        }

        // Resource Layouts
        {
            uint numParameters = (uint)desc.ResourceLayouts.Sum(static item => item.DX().GraphicsRootParameterCount);
            RootParameter* pRootParameters = Allocator.Alloc<RootParameter>(numParameters);

            uint offset = 0;
            for (int i = 0; i < desc.ResourceLayouts.Length; i++)
            {
                DXResourceLayout resourceLayout = desc.ResourceLayouts[i].DX();

                foreach (ShaderStages stage in DXHelpers.GraphicsShaderStages)
                {
                    if (resourceLayout.CalculateDescriptorTableRanges((uint)i,
                                                                      out DescriptorRange[] cbvSrvUavRanges,
                                                                      out DescriptorRange[] samplerRanges,
                                                                      stage))
                    {
                        if (cbvSrvUavRanges.Length > 0)
                        {
                            pRootParameters[offset++] = new()
                            {
                                ParameterType = RootParameterType.TypeDescriptorTable,
                                ShaderVisibility = DXFormats.GetShaderVisibility(stage),
                                DescriptorTable = new()
                                {
                                    NumDescriptorRanges = (uint)cbvSrvUavRanges.Length,
                                    PDescriptorRanges = Allocator.Alloc(cbvSrvUavRanges)
                                }
                            };
                        }

                        if (samplerRanges.Length > 0)
                        {
                            pRootParameters[offset++] = new()
                            {
                                ParameterType = RootParameterType.TypeDescriptorTable,
                                ShaderVisibility = DXFormats.GetShaderVisibility(stage),
                                DescriptorTable = new()
                                {
                                    NumDescriptorRanges = (uint)samplerRanges.Length,
                                    PDescriptorRanges = Allocator.Alloc(samplerRanges)
                                }
                            };
                        }
                    }
                }
            }

            RootSignatureDesc rootSignatureDesc = new()
            {
                NumParameters = numParameters,
                PParameters = pRootParameters,
                Flags = RootSignatureFlags.AllowInputAssemblerInputLayout
            };

            ComPtr<ID3D10Blob> blob = null;
            ComPtr<ID3D10Blob> error = null;

            Context.D3D12.SerializeRootSignature(&rootSignatureDesc,
                                                 D3DRootSignatureVersion.Version1,
                                                 ref blob,
                                                 ref error).ThrowIfError();

            Context.Device.CreateRootSignature(0,
                                               blob.GetBufferPointer(),
                                               blob.GetBufferSize(),
                                               out RootSignature).ThrowIfError();

            blob.Dispose();
            error.Dispose();

            graphicsPipelineStateDesc.PRootSignature = RootSignature;
        }

        // Primitive Topology
        {
            graphicsPipelineStateDesc.PrimitiveTopologyType = DXFormats.GetPrimitiveTopologyType(desc.PrimitiveTopology);
        }

        // Outputs
        {
            graphicsPipelineStateDesc.NumRenderTargets = (uint)desc.Outputs.ColorAttachments.Length;
            graphicsPipelineStateDesc.DSVFormat = desc.Outputs.DepthStencilAttachment is not null ? DXFormats.GetFormat(desc.Outputs.DepthStencilAttachment.Value) : Format.FormatUnknown;
            graphicsPipelineStateDesc.SampleDesc = DXFormats.GetSampleDesc(desc.Outputs.SampleCount);

            Span<Format> rtvFormats = graphicsPipelineStateDesc.RTVFormats.AsSpan();

            for (int i = 0; i < desc.Outputs.ColorAttachments.Length; i++)
            {
                rtvFormats[i] = DXFormats.GetFormat(desc.Outputs.ColorAttachments[i]);
            }
        }

        Context.Device.CreateGraphicsPipelineState(&graphicsPipelineStateDesc,
                                                   out PipelineState).ThrowIfError();

        VertexStrides = [.. desc.InputLayouts.Select(static item => item.Stride)];

        Allocator.Release();
    }

    public uint[] VertexStrides { get; }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public void Apply(ComPtr<ID3D12GraphicsCommandList> commandList)
    {
        commandList.SetPipelineState(PipelineState);
        commandList.SetGraphicsRootSignature(RootSignature);
        commandList.OMSetStencilRef((uint)Desc.RenderStates.StencilReference);
        commandList.IASetPrimitiveTopology(DXFormats.GetPrimitiveTopology(Desc.PrimitiveTopology));
    }

    public uint GetRootParameterOffset(uint slot)
    {
        return (uint)Desc.ResourceLayouts.Take((int)slot).Sum(static item => item.DX().GraphicsRootParameterCount);
    }

    protected override void SetName(string name)
    {
        PipelineState.SetName(name).ThrowIfError();
    }

    protected override void Destroy()
    {
        RootSignature.Dispose();
        PipelineState.Dispose();
    }
}
