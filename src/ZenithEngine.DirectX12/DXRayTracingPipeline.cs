using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXRayTracingPipeline : RayTracingPipeline
{
    public ComPtr<ID3D12RootSignature> RootSignature;
    public ComPtr<ID3D12PipelineState> PipelineState;

    public DXRayTracingPipeline(GraphicsContext context,
                                ref readonly RayTracingPipelineDesc desc) : base(context, in desc)
    {
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public void Apply(ComPtr<ID3D12GraphicsCommandList> commandList)
    {
        commandList.SetPipelineState(PipelineState);
        commandList.SetComputeRootSignature(RootSignature);
    }

    public uint GetRootParameterOffset(uint slot)
    {
        return (uint)Desc.ResourceLayouts.Take((int)slot).Sum(static item => item.DX().GlobalRootParameterCount);
    }

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
