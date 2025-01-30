using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXTexture : Texture
{
    public ComPtr<ID3D12Resource> Resource;

    private readonly ulong[] rowSizes;
    private readonly ResourceStates[] resourceStates;

    private CpuDescriptorHandle srv;
    private CpuDescriptorHandle uav;

    public DXTexture(GraphicsContext context,
                     ref readonly TextureDesc desc) : base(context, in desc)
    {
        ResourceDesc resourceDesc = new()
        {
            Dimension = DXFormats.GetResourceDimension(desc.Type),
            Alignment = 0,
            Width = desc.Width,
            Height = desc.Height,
            DepthOrArraySize = DXHelpers.GetDepthOrArraySize(desc),
            MipLevels = (ushort)desc.MipLevels,
            Format = DXFormats.GetFormat(desc.Format),
            SampleDesc = DXFormats.GetSampleDesc(desc.SampleCount),
            Layout = TextureLayout.LayoutUnknown,
            Flags = ResourceFlags.None
        };

        HeapProperties heapProperties = new(HeapType.Default);
        ResourceStates initialResourceState = ResourceStates.Common;
        DxClearValue* clearValue = null;

        if (desc.Usage.HasFlag(TextureUsage.Storage))
        {
            resourceDesc.Flags |= ResourceFlags.AllowUnorderedAccess;

            initialResourceState = ResourceStates.UnorderedAccess;
        }

        if (desc.Usage.HasFlag(TextureUsage.RenderTarget))
        {
            resourceDesc.Flags |= ResourceFlags.AllowRenderTarget;

            initialResourceState = ResourceStates.RenderTarget;

            clearValue = Allocator.Alloc<DxClearValue>();
            clearValue->Format = DXFormats.GetFormat(desc.Format);
        }

        if (desc.Usage.HasFlag(TextureUsage.DepthStencil))
        {
            resourceDesc.Flags |= ResourceFlags.AllowDepthStencil;

            initialResourceState = ResourceStates.DepthWrite;

            clearValue = Allocator.Alloc<DxClearValue>();
            clearValue->Format = DXFormats.GetFormat(desc.Format);
            clearValue->DepthStencil = new(1.0f, 0);
        }

        Context.Device.CreateCommittedResource(in heapProperties,
                                               HeapFlags.None,
                                               in resourceDesc,
                                               initialResourceState,
                                               clearValue,
                                               out Resource).ThrowIfError();

        uint subresourceCount = desc.MipLevels * DXHelpers.GetDepthOrArraySize(desc);

        rowSizes = new ulong[subresourceCount];
        for (int i = 0; i < rowSizes.Length; i++)
        {
            ulong rowSize;
            Context.Device.GetCopyableFootprints(in resourceDesc,
                                                 (uint)i,
                                                 1,
                                                 0,
                                                 null,
                                                 null,
                                                 &rowSize,
                                                 null);

            rowSizes[i] = rowSize;
        }

        resourceStates = new ResourceStates[subresourceCount];
        Array.Fill(resourceStates, initialResourceState);

        Allocator.Release();
    }

    public ref readonly CpuDescriptorHandle Srv
    {
        get
        {
            if (srv.Ptr is 0)
            {
                InitSrv();
            }

            return ref srv;
        }
    }

    public ref readonly CpuDescriptorHandle Uav
    {
        get
        {
            if (uav.Ptr is 0)
            {
                InitUav();
            }

            return ref uav;
        }
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Resource.SetName(name).ThrowIfError();
    }

    protected override void Destroy()
    {
        if (uav.Ptr is not 0)
        {
            Context.CbvSrvUavAllocator!.Free(uav);
        }

        if (srv.Ptr is not 0)
        {
            Context.CbvSrvUavAllocator!.Free(srv);
        }

        Resource.Dispose();
    }

    private void InitSrv()
    {
        ShaderResourceViewDesc desc = new()
        {
            Format = DXFormats.GetFormat(Desc.Format),
            Shader4ComponentMapping = DXGraphicsContext.DefaultShader4ComponentMapping
        };

        bool isMultiSampled = Desc.SampleCount is not TextureSampleCount.Count1;

        switch (Desc.Type)
        {
            case TextureType.Texture1D:
                {
                    desc.ViewDimension = SrvDimension.Texture1D;
                    desc.Texture1D.MipLevels = Desc.MipLevels;
                }
                break;
            case TextureType.Texture1DArray:
                {
                    desc.ViewDimension = SrvDimension.Texture1Darray;
                    desc.Texture1DArray.MipLevels = Desc.MipLevels;
                    desc.Texture1DArray.ArraySize = Desc.ArrayLayers;
                }
                break;
            case TextureType.Texture2D:
                {
                    desc.ViewDimension = isMultiSampled ? SrvDimension.Texture2Dms : SrvDimension.Texture2D;
                    desc.Texture2D.MipLevels = Desc.MipLevels;
                }
                break;
            case TextureType.Texture2DArray:
                {
                    if (isMultiSampled)
                    {
                        desc.ViewDimension = SrvDimension.Texture2Dmsarray;
                        desc.Texture2DMSArray.ArraySize = Desc.ArrayLayers;
                    }
                    else
                    {
                        desc.ViewDimension = SrvDimension.Texture2Darray;
                        desc.Texture2DArray.MipLevels = Desc.MipLevels;
                        desc.Texture2DArray.ArraySize = Desc.ArrayLayers;
                    }
                }
                break;
            case TextureType.Texture3D:
                {
                    desc.ViewDimension = SrvDimension.Texture3D;
                    desc.Texture3D.MipLevels = Desc.MipLevels;
                }
                break;
            case TextureType.TextureCube:
                {
                    desc.ViewDimension = SrvDimension.Texturecube;
                    desc.TextureCube.MipLevels = Desc.MipLevels;
                }
                break;
            case TextureType.TextureCubeArray:
                {
                    desc.ViewDimension = SrvDimension.Texturecubearray;
                    desc.TextureCubeArray.MipLevels = Desc.MipLevels;
                    desc.TextureCubeArray.NumCubes = Desc.ArrayLayers;
                }
                break;
            default:
                throw new ZenithEngineException(ExceptionHelpers.NotSupported(Desc.Type));
        }

        srv = Context.CbvSrvUavAllocator!.Alloc();

        Context.Device.CreateShaderResourceView(Resource, in desc, srv);
    }

    private void InitUav()
    {
        UnorderedAccessViewDesc desc = new()
        {
            Format = DXFormats.GetFormat(Desc.Format)
        };

        switch (Desc.Type)
        {
            case TextureType.Texture1D:
                {
                    desc.ViewDimension = UavDimension.Texture1D;
                }
                break;
            case TextureType.Texture1DArray:
                {
                    desc.ViewDimension = UavDimension.Texture1Darray;
                    desc.Texture1DArray.ArraySize = Desc.ArrayLayers;
                }
                break;
            case TextureType.Texture2D:
                {
                    desc.ViewDimension = UavDimension.Texture2D;
                }
                break;
            case TextureType.Texture2DArray:
            case TextureType.TextureCube:
            case TextureType.TextureCubeArray:
                {
                    desc.ViewDimension = UavDimension.Texture2Darray;
                    desc.Texture2DArray.ArraySize = Desc.ArrayLayers;
                }
                break;
            case TextureType.Texture3D:
                {
                    desc.ViewDimension = UavDimension.Texture3D;
                    desc.Texture3D.WSize = Desc.Depth;
                }
                break;
            default:
                throw new ZenithEngineException(ExceptionHelpers.NotSupported(Desc.Type));
        }

        uav = Context.CbvSrvUavAllocator!.Alloc();

        Context.Device.CreateUnorderedAccessView(Resource, null, in desc, uav);
    }
}
