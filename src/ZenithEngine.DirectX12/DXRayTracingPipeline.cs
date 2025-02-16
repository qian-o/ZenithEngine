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
        uint index = 0;
        uint numSubObjects = (uint)(1 + desc.HitGroups.Length + 1 + 2);
        StateSubobject* pSubobjects = Allocator.Alloc<StateSubobject>(numSubObjects);

        StateObjectDesc stateObjectDesc = new()
        {
            Type = StateObjectType.RaytracingPipeline,
            NumSubobjects = numSubObjects,
            PSubobjects = pSubobjects
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

            DxilLibraryDesc dxilLibraryDesc = new()
            {
                DXILLibrary = desc.Shaders.RayGen.DX().Shader,
                NumExports = (uint)shaders.Length,
                PExports = Allocator.Alloc([.. shaders.Select(item => new ExportDesc
                {
                    Name = (char*)Allocator.AllocUni(item.Desc.EntryPoint),
                    ExportToRename = null,
                    Flags = ExportFlags.None
                })])
            };

            pSubobjects[index++] = new()
            {
                Type = StateSubobjectType.DxilLibrary,
                PDesc = &dxilLibraryDesc
            };

            uint groupCount = (uint)desc.HitGroups.Length;
            DxHitGroupDesc* groups = Allocator.Alloc<DxHitGroupDesc>(groupCount);

            for (uint i = 0; i < groupCount; i++)
            {
                HitGroupDesc hitGroup = desc.HitGroups[i];

                char* anyHit = hitGroup.AnyHit is not null ? (char*)Allocator.AllocUni(hitGroup.AnyHit) : null;
                char* closestHit = hitGroup.ClosestHit is not null ? (char*)Allocator.AllocUni(hitGroup.ClosestHit) : null;
                char* intersection = hitGroup.Intersection is not null ? (char*)Allocator.AllocUni(hitGroup.Intersection) : null;

                groups[i] = new()
                {
                    HitGroupExport = (char*)Allocator.AllocUni(hitGroup.Name),
                    Type = DXFormats.GetHitGroupType(hitGroup.Type),
                    AnyHitShaderImport = anyHit,
                    ClosestHitShaderImport = closestHit,
                    IntersectionShaderImport = intersection
                };

                pSubobjects[index++] = new()
                {
                    Type = StateSubobjectType.HitGroup,
                    PDesc = groups + i
                };
            }
        }

        // Resource Layouts
        {
            uint numParameters = (uint)desc.ResourceLayouts.Sum(static item => item.DX().GlobalRootParameterCount);
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
                Flags = RootSignatureFlags.None
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

            GlobalRootSignature globalRootSignature = new()
            {
                PGlobalRootSignature = RootSignature
            };

            pSubobjects[index++] = new()
            {
                Type = StateSubobjectType.GlobalRootSignature,
                PDesc = &globalRootSignature
            };
        }

        // Other Config
        {
            RaytracingPipelineConfig raytracingPipelineConfig = new()
            {
                MaxTraceRecursionDepth = desc.MaxTraceRecursionDepth
            };

            pSubobjects[index++] = new()
            {
                Type = StateSubobjectType.RaytracingPipelineConfig,
                PDesc = &raytracingPipelineConfig
            };

            RaytracingShaderConfig raytracingShaderConfig = new()
            {
                MaxPayloadSizeInBytes = desc.MaxPayloadSizeInBytes,
                MaxAttributeSizeInBytes = desc.MaxAttributeSizeInBytes
            };

            pSubobjects[index++] = new()
            {
                Type = StateSubobjectType.RaytracingShaderConfig,
                PDesc = &raytracingShaderConfig
            };
        }

        Context.Device5.CreateStateObject(&stateObjectDesc, out StateObject).ThrowIfError();

        ShaderTable = new(Context,
                          StateObject,
                          [desc.Shaders.RayGen.Desc.EntryPoint],
                          [.. desc.Shaders.Miss.Select(static item => item.Desc.EntryPoint)],
                          [.. desc.HitGroups.Select(static item => item.Name)]);

        Allocator.Release();
    }

    public DXShaderTable ShaderTable { get; }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public void Apply(ComPtr<ID3D12GraphicsCommandList4> commandList)
    {
        commandList.SetPipelineState1(StateObject);
        commandList.SetComputeRootSignature(RootSignature);
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
        ShaderTable.Dispose();

        RootSignature.Dispose();
        StateObject.Dispose();
    }
}
