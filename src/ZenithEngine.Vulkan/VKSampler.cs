using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKSampler : Sampler
{
    public VkSampler Sampler;

    public VKSampler(GraphicsContext context,
                     ref readonly SamplerDesc desc) : base(context, in desc)
    {
        VKFormats.GetFilter(desc.Filter,
                            out Filter minFilter,
                            out Filter magFilter,
                            out SamplerMipmapMode mode);

        bool compareEnable = desc.ComparisonFunction.HasValue;

        SamplerCreateInfo createInfo = new()
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = magFilter,
            MinFilter = minFilter,
            MipmapMode = mode,
            AddressModeU = VKFormats.GetSamplerAddressMode(desc.AddressModeU),
            AddressModeV = VKFormats.GetSamplerAddressMode(desc.AddressModeV),
            AddressModeW = VKFormats.GetSamplerAddressMode(desc.AddressModeW),
            MipLodBias = desc.LodBias,
            AnisotropyEnable = desc.Filter == SamplerFilter.Anisotropic,
            MaxAnisotropy = desc.MaximumAnisotropy,
            CompareEnable = compareEnable,
            CompareOp = compareEnable ? VKFormats.GetCompareOp(desc.ComparisonFunction!.Value) : CompareOp.Never,
            MinLod = desc.MinimumLod,
            MaxLod = desc.MaximumLod,
            BorderColor = VKFormats.GetBorderColor(desc.BorderColor)
        };

        Context.Vk.CreateSampler(Context.Device, &createInfo, null, out Sampler).ThrowIfError();
    }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.Sampler, Sampler.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroySampler(Context.Device, Sampler, null);
    }
}
