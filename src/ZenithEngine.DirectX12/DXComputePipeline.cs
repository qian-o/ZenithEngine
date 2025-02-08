using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXComputePipeline : ComputePipeline
{
    public ComPtr<ID3D12RootSignature> RootSignature;
    public ComPtr<ID3D12PipelineState> PipelineState;

    public DXComputePipeline(GraphicsContext context,
                             ref readonly ComputePipelineDesc desc) : base(context, in desc)
    {
        ComputePipelineStateDesc computePipelineStateDesc = new();

        // Shader
        {
            computePipelineStateDesc.CS = desc.Shader.DX().Shader;
        }

        // Resource Layouts
        {
            uint numParameters = (uint)desc.ResourceLayouts.Sum(static item => item.DX().AllStagesRootParameterCount);
            RootParameter* pRootParameters = Allocator.Alloc<RootParameter>(numParameters);

            uint offset = 0;
            for (int i = 0; i < desc.ResourceLayouts.Length; i++)
            {
                DXResourceLayout resourceLayout = desc.ResourceLayouts[i].DX();

                if (resourceLayout.CalculateDescriptorTableRanges((uint)i,
                                                                  out DescriptorRange[] cbvSrvUavRanges,
                                                                  out DescriptorRange[] samplerRanges))
                {
                    if (cbvSrvUavRanges.Length > 0)
                    {
                        pRootParameters[offset++] = new()
                        {
                            ParameterType = RootParameterType.TypeDescriptorTable,
                            ShaderVisibility = ShaderVisibility.All,
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
                            ShaderVisibility = ShaderVisibility.All,
                            DescriptorTable = new()
                            {
                                NumDescriptorRanges = (uint)samplerRanges.Length,
                                PDescriptorRanges = Allocator.Alloc(samplerRanges)
                            }
                        };
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

            computePipelineStateDesc.PRootSignature = RootSignature;
        }

        Context.Device.CreateComputePipelineState(&computePipelineStateDesc,
                                                  out PipelineState).ThrowIfError();

        Allocator.Release();
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        PipelineState.SetName(name).ThrowIfError();
    }

    protected override void Destroy()
    {
        RootSignature.Dispose();
        PipelineState.Dispose();
    }
}
