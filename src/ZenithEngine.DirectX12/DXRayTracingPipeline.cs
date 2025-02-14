using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXRayTracingPipeline : RayTracingPipeline
{
    public ComPtr<ID3D12RootSignature> RootSignature;
    public ComPtr<ID3D12StateObject> StateObject;

    public DXRayTracingPipeline(GraphicsContext context,
                                ref readonly RayTracingPipelineDesc desc) : base(context, in desc)
    {
        Context.Device.QueryInterface(out ComPtr<ID3D12Device5> device5).ThrowIfError();

        StateObjectDesc stateObjectDesc = new()
        {
            Type = StateObjectType.RaytracingPipeline
        };

        List<StateSubobject> objects = [];

        // Shaders and Hit Groups
        {
            Shader[] shaders =
            [
                .. desc.Shaders.Miss,
                .. desc.Shaders.ClosestHit,
                .. desc.Shaders.AnyHit,
                .. desc.Shaders.Intersection
            ];

            DxilLibraryDesc dxilLibraryDesc = new()
            {
                DXILLibrary = desc.Shaders.RayGen.DX().Shader,
                NumExports = (uint)shaders.Length,
                PExports = Allocator.Alloc([.. shaders.Select(item => new ExportDesc
                {
                    Name = (char*)Allocator.AllocUTF8(item.Desc.EntryPoint),
                    ExportToRename = null,
                    Flags = ExportFlags.None
                })])
            };

            objects.Add(new()
            {
                Type = StateSubobjectType.DxilLibrary,
                PDesc = &dxilLibraryDesc
            });
        }

        device5.Dispose();
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public void Apply(ComPtr<ID3D12GraphicsCommandList> commandList)
    {
        commandList.QueryInterface(out ComPtr<ID3D12GraphicsCommandList4> commandList4).ThrowIfError();

        commandList4.SetPipelineState1(StateObject);
        commandList4.SetComputeRootSignature(RootSignature);

        commandList4.Dispose();
    }

    public uint GetRootParameterOffset(uint slot)
    {
        return (uint)Desc.ResourceLayouts.Take((int)slot).Sum(static item => item.DX().GlobalRootParameterCount);
    }

    protected override void DebugName(string name)
    {
        StateObject.SetName(name).ThrowIfError();
    }

    protected override void Destroy()
    {
        RootSignature.Dispose();
        StateObject.Dispose();
    }
}
