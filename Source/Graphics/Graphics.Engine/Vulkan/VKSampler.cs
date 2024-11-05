using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKSampler : Sampler
{
    public VKSampler(Context context,
                     ref readonly SamplerDescription description) : base(context, in description)
    {
        Formats.GetFilter(description.Filter,
                          out Filter minFilter,
                          out Filter magFilter,
                          out SamplerMipmapMode mipFilter);

        bool compareEnable = description.ComparisonKind.HasValue;

        SamplerCreateInfo createInfo = new()
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = magFilter,
            MinFilter = minFilter,
            MipmapMode = mipFilter,
            AddressModeU = Formats.GetSamplerAddressMode(description.AddressModeU),
            AddressModeV = Formats.GetSamplerAddressMode(description.AddressModeV),
            AddressModeW = Formats.GetSamplerAddressMode(description.AddressModeW),
            CompareEnable = compareEnable,
            CompareOp = compareEnable ? Formats.GetCompareOp(description.ComparisonKind!.Value) : CompareOp.Never,
            AnisotropyEnable = description.Filter == SamplerFilter.Anisotropic,
            MaxAnisotropy = description.MaximumAnisotropy,
            MinLod = description.MinimumLod,
            MaxLod = description.MaximumLod,
            MipLodBias = description.LodBias,
            BorderColor = Formats.GetBorderColor(description.BorderColor)
        };

        VkSampler sampler;
        Context.Vk.CreateSampler(Context.Device, &createInfo, null, &sampler).ThrowCode();

        Sampler = sampler;
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkSampler Sampler { get; }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.Sampler, Sampler.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroySampler(Context.Device, Sampler, null);
    }
}
