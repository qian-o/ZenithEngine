using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXResourceSet : ResourceSet
{
    public DXResourceSet(GraphicsContext context,
                         ref readonly ResourceSetDesc desc) : base(context, in desc)
    {
        uint resourceOffset = 0;
        List<DXTexture> srvTextures = [];
        List<DXTexture> uavTextures = [];

        ResourceLayoutDesc layoutDesc = desc.Layout.Desc;

        for (int i = 0; i < layoutDesc.Elements.Length; i++)
        {
            ResourceElementDesc element = layoutDesc.Elements[i];
            GraphicsResource[] resources = desc.Resources[(int)resourceOffset..(int)(resourceOffset + element.Count)];

            if (element.Type is ResourceType.Texture or ResourceType.TextureReadWrite)
            {
                bool isSrv = element.Type is ResourceType.Texture;

                for (uint j = 0; j < element.Count; j++)
                {
                    if (isSrv)
                    {
                        srvTextures.Add((DXTexture)resources[j]);
                    }
                    else
                    {
                        uavTextures.Add((DXTexture)resources[j]);
                    }
                }
            }

            resourceOffset += element.Count;
        }

        SrvTextures = [.. srvTextures];
        UavTextures = [.. uavTextures];
    }

    public DXTexture[] SrvTextures { get; }

    public DXTexture[] UavTextures { get; }

    public void Bind(ComPtr<ID3D12GraphicsCommandList> commandList,
                     DXDescriptorTableAllocator cbvSrvUavAllocator,
                     DXDescriptorTableAllocator samplerAllocator,
                     bool isGraphics,
                     uint rootParameterOffset)
    {
        DXResourceLayout layout = Desc.Layout.DX();

        if (isGraphics)
        {
            foreach (ShaderStages stage in DXHelpers.GraphicsShaderStages)
            {
                if (layout.CalculateResourceBindings(out DXResourceBinding[] cbvSrvUavBindings,
                                                     out DXResourceBinding[] samplerBindings,
                                                     stage))
                {
                    if (cbvSrvUavBindings.Length > 0)
                    {
                        commandList.SetGraphicsRootDescriptorTable(rootParameterOffset++,
                                                                   cbvSrvUavAllocator.GetCurrentTableHandle());

                        UpdateDescriptorTable(cbvSrvUavAllocator, cbvSrvUavBindings);
                    }

                    if (samplerBindings.Length > 0)
                    {
                        commandList.SetGraphicsRootDescriptorTable(rootParameterOffset++,
                                                                   samplerAllocator.GetCurrentTableHandle());

                        UpdateDescriptorTable(samplerAllocator, samplerBindings);
                    }
                }
            }
        }
        else if (layout.CalculateResourceBindings(out DXResourceBinding[] cbvSrvUavBindings,
                                                  out DXResourceBinding[] samplerBindings))
        {
            if (cbvSrvUavBindings.Length > 0)
            {
                commandList.SetComputeRootDescriptorTable(rootParameterOffset++,
                                                          cbvSrvUavAllocator.GetCurrentTableHandle());

                UpdateDescriptorTable(cbvSrvUavAllocator, cbvSrvUavBindings);
            }

            if (samplerBindings.Length > 0)
            {
                commandList.SetComputeRootDescriptorTable(rootParameterOffset++,
                                                          samplerAllocator.GetCurrentTableHandle());

                UpdateDescriptorTable(samplerAllocator, samplerBindings);
            }
        }
    }

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
        Array.Clear(UavTextures, 0, UavTextures.Length);
        Array.Clear(SrvTextures, 0, SrvTextures.Length);
    }

    private void UpdateDescriptorTable(DXDescriptorTableAllocator allocator, DXResourceBinding[] bindings)
    {
        List<CpuDescriptorHandle> handles = [];

        foreach (DXResourceBinding binding in bindings)
        {
            foreach (uint index in binding.Indices)
            {
                GraphicsResource resource = Desc.Resources[index];

                switch (binding.Type)
                {
                    case ResourceType.ConstantBuffer:
                        {
                            handles.Add(((DXBuffer)resource).Cbv);
                        }
                        break;
                    case ResourceType.StructuredBuffer:
                        {
                            handles.Add(((DXBuffer)resource).Srv);
                        }
                        break;
                    case ResourceType.StructuredBufferReadWrite:
                        {
                            handles.Add(((DXBuffer)resource).Uav);
                        }
                        break;
                    case ResourceType.Texture:
                        {
                            handles.Add(((DXTexture)resource).Srv);
                        }
                        break;
                    case ResourceType.TextureReadWrite:
                        {
                            handles.Add(((DXTexture)resource).Uav);
                        }
                        break;
                    case ResourceType.Sampler:
                        {
                            handles.Add(((DXSampler)resource).Handle);
                        }
                        break;
                    case ResourceType.AccelerationStructure:
                        {
                            handles.Add(((DXTopLevelAS)resource).Srv);
                        }
                        break;
                    default:
                        throw new ZenithEngineException(ExceptionHelpers.NotSupported(binding.Type));
                }
            }
        }

        allocator.UpdateDescriptors([.. handles]);
    }
}
