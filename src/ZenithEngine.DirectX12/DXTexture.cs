using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal class DXTexture : Texture
{
    public ComPtr<ID3D12Resource> Resource;

    private readonly ResourceStates[] resourceStates;

    private CpuDescriptorHandle rtv;
    private CpuDescriptorHandle dsv;
    private CpuDescriptorHandle srv;
    private CpuDescriptorHandle uav;

    public DXTexture(GraphicsContext context,
                     ref readonly TextureDesc desc) : base(context, in desc)
    {
        ResourceDesc resourceDesc = new()
        {
        };
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Resource.SetName(name);
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

        if (dsv.Ptr is not 0)
        {
            Context.DsvAllocator!.Free(dsv);
        }

        if (rtv.Ptr is not 0)
        {
            Context.RtvAllocator!.Free(rtv);
        }

        Resource.Dispose();
    }
}
