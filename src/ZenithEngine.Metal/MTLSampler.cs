using SharpMetal.Metal;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLSampler : Sampler
{
    public MTLSampler(GraphicsContext context, ref readonly SamplerDesc desc) : base(context, in desc)
    {
        using MTLSamplerDescriptor samplerDesc = new()
        {
            MinFilter = MTLFormats.GetMTLSamplerMinMagFilter(desc.Filter),
            MagFilter = MTLFormats.GetMTLSamplerMinMagFilter(desc.Filter),
            MipFilter = MTLFormats.GetMTLSamplerMipFilter(desc.Filter),
            AddressModeS = MTLFormats.GetMTLSamplerAddressMode(desc.AddressModeU),
            AddressModeT = MTLFormats.GetMTLSamplerAddressMode(desc.AddressModeV),
            AddressModeR = MTLFormats.GetMTLSamplerAddressMode(desc.AddressModeW),
            MaxAnisotropy = desc.MaximumAnisotropy,
            CompareFunction = MTLFormats.GetMTLCompareFunction(desc.ComparisonFunction),
            LodMinClamp = desc.MinimumLod,
            LodMaxClamp = desc.MaximumLod
        };

        Sampler = Context.Device.NewSamplerState(samplerDesc)!;
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public SharpMetal.Metal.MTLSamplerState Sampler { get; }

    protected override void SetName(string name)
    {
        // Sampler.Label is readonly in SharpMetal
        // Label must be set on the descriptor before sampler creation
    }

    protected override void Destroy()
    {
        Sampler.Dispose();
    }
}
