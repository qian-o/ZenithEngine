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

        Sampler = Context.Device.CreateSamplerState(samplerDesc)!;
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public MTLSamplerState Sampler { get; }

    protected override void SetName(string name)
    {
        Sampler.Label = name;
    }

    protected override void Destroy()
    {
        Sampler.Dispose();
    }
}
