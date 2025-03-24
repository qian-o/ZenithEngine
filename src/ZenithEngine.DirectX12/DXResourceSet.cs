using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
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
            LayoutElementDesc element = layoutDesc.Elements[i];
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

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public void Bind(ComPtr<ID3D12GraphicsCommandList> commandList,
                     DXDescriptorTableAllocator cbvSrvUavAllocator,
                     DXDescriptorTableAllocator samplerAllocator,
                     bool isGraphics,
                     uint rootParameterOffset,
                     uint[] bufferOffsets)
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

                        UpdateDescriptorTable(cbvSrvUavAllocator,
                                              samplerAllocator,
                                              cbvSrvUavBindings,
                                              bufferOffsets);
                    }

                    if (samplerBindings.Length > 0)
                    {
                        commandList.SetGraphicsRootDescriptorTable(rootParameterOffset++,
                                                                   samplerAllocator.GetCurrentTableHandle());

                        UpdateDescriptorTable(cbvSrvUavAllocator,
                                              samplerAllocator,
                                              samplerBindings,
                                              bufferOffsets);
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

                UpdateDescriptorTable(cbvSrvUavAllocator,
                                      samplerAllocator,
                                      cbvSrvUavBindings,
                                      bufferOffsets);
            }

            if (samplerBindings.Length > 0)
            {
                commandList.SetComputeRootDescriptorTable(rootParameterOffset++,
                                                          samplerAllocator.GetCurrentTableHandle());

                UpdateDescriptorTable(cbvSrvUavAllocator,
                                      samplerAllocator,
                                      samplerBindings,
                                      bufferOffsets);
            }
        }
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        Array.Clear(UavTextures, 0, UavTextures.Length);
        Array.Clear(SrvTextures, 0, SrvTextures.Length);
    }

    private void UpdateDescriptorTable(DXDescriptorTableAllocator cbvSrvUavAllocator,
                                       DXDescriptorTableAllocator samplerAllocator,
                                       DXResourceBinding[] bindings,
                                       uint[] bufferOffsets)
    {
        uint offset = 0;
        foreach (DXResourceBinding binding in bindings)
        {
            foreach (uint index in binding.Indices)
            {
                GraphicsResource resource = Desc.Resources[index];

                switch (binding.Type)
                {
                    case ResourceType.ConstantBuffer:
                        {
                            DXBuffer buffer = (DXBuffer)resource;

                            if (binding.DynamicOffsetIndex is not -1)
                            {
                                uint offsetInBytes = bufferOffsets[binding.DynamicOffsetIndex];
                                uint sizeInBytes = binding.Range is not 0 ? Utils.AlignedSize(binding.Range, 256u) : buffer.SizeInBytes - offsetInBytes;

                                ConstantBufferViewDesc desc = new()
                                {
                                    BufferLocation = buffer.Resource.GetGPUVirtualAddress() + offsetInBytes,
                                    SizeInBytes = sizeInBytes
                                };

                                Context.Device.CreateConstantBufferView(&desc,
                                                                        cbvSrvUavAllocator.UpdateDescriptorHandle());
                            }
                            else
                            {
                                cbvSrvUavAllocator.UpdateDescriptor(buffer.Cbv);
                            }
                        }
                        break;
                    case ResourceType.StructuredBuffer:
                        {
                            DXBuffer buffer = (DXBuffer)resource;

                            if (binding.DynamicOffsetIndex is not -1)
                            {
                                uint offsetInBytes = bufferOffsets[binding.DynamicOffsetIndex];
                                uint sizeInBytes = binding.Range is not 0 ? binding.Range : buffer.SizeInBytes - offsetInBytes;

                                ShaderResourceViewDesc desc = new()
                                {
                                    Format = Format.FormatUnknown,
                                    ViewDimension = SrvDimension.Buffer,
                                    Shader4ComponentMapping = DXGraphicsContext.DefaultShader4ComponentMapping,
                                    Buffer = new()
                                    {
                                        FirstElement = offsetInBytes / buffer.Desc.StructureStrideInBytes,
                                        NumElements = sizeInBytes / buffer.Desc.StructureStrideInBytes,
                                        StructureByteStride = buffer.Desc.StructureStrideInBytes,
                                        Flags = BufferSrvFlags.None
                                    }
                                };

                                Context.Device.CreateShaderResourceView(buffer.Resource,
                                                                        &desc,
                                                                        cbvSrvUavAllocator.UpdateDescriptorHandle());
                            }
                            else
                            {
                                cbvSrvUavAllocator.UpdateDescriptor(buffer.Srv);
                            }
                        }
                        break;
                    case ResourceType.StructuredBufferReadWrite:
                        {
                            DXBuffer buffer = (DXBuffer)resource;

                            if (binding.DynamicOffsetIndex is not -1)
                            {
                                uint offsetInBytes = bufferOffsets[binding.DynamicOffsetIndex];
                                uint sizeInBytes = binding.Range is not 0 ? binding.Range : buffer.SizeInBytes - offsetInBytes;

                                UnorderedAccessViewDesc desc = new()
                                {
                                    Format = Format.FormatUnknown,
                                    ViewDimension = UavDimension.Buffer,
                                    Buffer = new()
                                    {
                                        FirstElement = offsetInBytes / buffer.Desc.StructureStrideInBytes,
                                        NumElements = sizeInBytes / buffer.Desc.StructureStrideInBytes,
                                        StructureByteStride = buffer.Desc.StructureStrideInBytes,
                                        CounterOffsetInBytes = 0,
                                        Flags = BufferUavFlags.None
                                    }
                                };

                                Context.Device.CreateUnorderedAccessView(buffer.Resource,
                                                                         (ID3D12Resource*)null,
                                                                         &desc,
                                                                         cbvSrvUavAllocator.UpdateDescriptorHandle());
                            }
                            else
                            {
                                cbvSrvUavAllocator.UpdateDescriptor(buffer.Uav);
                            }
                        }
                        break;
                    case ResourceType.Texture:
                        {
                            cbvSrvUavAllocator.UpdateDescriptor(((DXTexture)resource).Srv);
                        }
                        break;
                    case ResourceType.TextureReadWrite:
                        {
                            cbvSrvUavAllocator.UpdateDescriptor(((DXTexture)resource).Uav);
                        }
                        break;
                    case ResourceType.Sampler:
                        {
                            samplerAllocator.UpdateDescriptor(((DXSampler)resource).Handle);
                        }
                        break;
                    case ResourceType.AccelerationStructure:
                        {
                            cbvSrvUavAllocator.UpdateDescriptor(((DXTopLevelAS)resource).Srv);
                        }
                        break;
                    default:
                        throw new ZenithEngineException(ExceptionHelpers.NotSupported(binding.Type));
                }

                offset++;
            }
        }
    }
}
