using Silk.NET.Direct3D12;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXSampler : Sampler
{
    public CpuDescriptorHandle Handle;

    public DXSampler(GraphicsContext context,
                     ref readonly SamplerDesc desc) : base(context, in desc)
    {
        DxSamplerDesc samplerDesc = new()
        {
            Filter = DXFormats.GetFilter(desc.Filter, desc.ComparisonFunction is not ComparisonFunction.Never),
            AddressU = DXFormats.GetTextureAddressMode(desc.AddressModeU),
            AddressV = DXFormats.GetTextureAddressMode(desc.AddressModeV),
            AddressW = DXFormats.GetTextureAddressMode(desc.AddressModeW),
            MipLODBias = desc.LodBias,
            MaxAnisotropy = desc.MaximumAnisotropy,
            ComparisonFunc = DXFormats.GetComparisonFunc(desc.ComparisonFunction),
            MinLOD = desc.MinimumLod,
            MaxLOD = desc.MaximumLod
        };

        switch (desc.BorderColor)
        {
            case SamplerBorderColor.TransparentBlack:
                {
                    samplerDesc.BorderColor[0] = 0;
                    samplerDesc.BorderColor[1] = 0;
                    samplerDesc.BorderColor[2] = 0;
                    samplerDesc.BorderColor[3] = 0;
                }
                break;
            case SamplerBorderColor.OpaqueBlack:
                {
                    samplerDesc.BorderColor[0] = 0;
                    samplerDesc.BorderColor[1] = 0;
                    samplerDesc.BorderColor[2] = 0;
                    samplerDesc.BorderColor[3] = 1;
                }
                break;
            case SamplerBorderColor.OpaqueWhite:
                {
                    samplerDesc.BorderColor[0] = 1;
                    samplerDesc.BorderColor[1] = 1;
                    samplerDesc.BorderColor[2] = 1;
                    samplerDesc.BorderColor[3] = 1;
                }
                break;
            default:
                throw new ZenithEngineException(ExceptionHelpers.NotSupported(desc.BorderColor));
        }

        Handle = Context.SamplerAllocator!.Alloc();

        Context.Device.CreateSampler(&samplerDesc, Handle);
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        Context.SamplerAllocator!.Free(Handle);
    }
}
