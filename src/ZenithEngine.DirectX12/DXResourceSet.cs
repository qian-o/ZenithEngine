using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal class DXResourceSet : ResourceSet
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

        DynamicConstantBufferCount = layoutDesc.DynamicConstantBufferCount;
        SrvTextures = [.. srvTextures];
        UavTextures = [.. uavTextures];
    }

    public uint DynamicConstantBufferCount { get; }

    public DXTexture[] SrvTextures { get; }

    public DXTexture[] UavTextures { get; }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public void Bind(ComPtr<ID3D12GraphicsCommandList> commandList,
                     DXDescriptorTableAllocator cbvSrvUavAllocator,
                     DXDescriptorTableAllocator samplerAllocator,
                     bool isGraphics,
                     uint rootParameterOffset,
                     uint[]? constantBufferOffsets)
    {
        DXResourceLayout layout = Desc.Layout.DX();

        if (isGraphics)
        {
            foreach (ShaderStages stage in DXHelpers.GraphicsShaderStages)
            {
                if (layout.CalculateResourceIndices(out DXResourceIndices[] cbvSrvUavIndices,
                                                    out DXResourceIndices[] samplerIndices,
                                                    stage))
                {
                    if (cbvSrvUavIndices.Length > 0)
                    {
                        CpuDescriptorHandle[] handles = GetDescriptorHandles(cbvSrvUavIndices,
                                                                             constantBufferOffsets);

                        commandList.SetGraphicsRootDescriptorTable(rootParameterOffset++,
                                                                   cbvSrvUavAllocator.Alloc(handles));
                    }

                    if (samplerIndices.Length > 0)
                    {
                        CpuDescriptorHandle[] handles = GetDescriptorHandles(samplerIndices,
                                                                             constantBufferOffsets);

                        commandList.SetGraphicsRootDescriptorTable(rootParameterOffset++,
                                                                   samplerAllocator.Alloc(handles));
                    }
                }
            }
        }
        else if (layout.CalculateResourceIndices(out DXResourceIndices[] cbvSrvUavIndices,
                                                 out DXResourceIndices[] samplerIndices))
        {
            if (cbvSrvUavIndices.Length > 0)
            {
                CpuDescriptorHandle[] handles = GetDescriptorHandles(cbvSrvUavIndices,
                                                                     constantBufferOffsets);

                commandList.SetGraphicsRootDescriptorTable(rootParameterOffset++,
                                                           cbvSrvUavAllocator.Alloc(handles));
            }

            if (samplerIndices.Length > 0)
            {
                CpuDescriptorHandle[] handles = GetDescriptorHandles(samplerIndices,
                                                                     constantBufferOffsets);

                commandList.SetGraphicsRootDescriptorTable(rootParameterOffset++,
                                                           samplerAllocator.Alloc(handles));
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

    private CpuDescriptorHandle[] GetDescriptorHandles(DXResourceIndices[] indices,
                                                       uint[]? constantBufferOffsets)
    {
        throw new NotImplementedException();
    }
}
