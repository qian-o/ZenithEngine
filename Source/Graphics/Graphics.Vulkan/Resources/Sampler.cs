using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Sampler : DeviceResource, IBindableResource
{
    private readonly VkSampler _sampler;

    internal Sampler(GraphicsDevice graphicsDevice, ref readonly SamplerDescription description) : base(graphicsDevice)
    {
        Formats.GetFilter(description.Filter, out Filter minFilter, out Filter magFilter, out SamplerMipmapMode mipFilter);

        SamplerCreateInfo samplerCreateInfo = new()
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = magFilter,
            MinFilter = minFilter,
            MipmapMode = mipFilter,
            AddressModeU = Formats.GetSamplerAddressMode(description.AddressModeU),
            AddressModeV = Formats.GetSamplerAddressMode(description.AddressModeV),
            AddressModeW = Formats.GetSamplerAddressMode(description.AddressModeW),
            CompareEnable = description.ComparisonKind.HasValue,
            CompareOp = description.ComparisonKind.HasValue ? Formats.GetCompareOp(description.ComparisonKind.Value) : CompareOp.Never,
            AnisotropyEnable = description.Filter == SamplerFilter.Anisotropic,
            MaxAnisotropy = description.MaximumAnisotropy,
            MinLod = description.MinimumLod,
            MaxLod = description.MaximumLod,
            MipLodBias = description.LodBias,
            BorderColor = Formats.GetBorderColor(description.BorderColor),
        };

        VkSampler sampler;
        Vk.CreateSampler(Device, &samplerCreateInfo, null, &sampler).ThrowCode();

        _sampler = sampler;
    }

    internal VkSampler Handle => _sampler;

    protected override void Destroy()
    {
        Vk.DestroySampler(Device, _sampler, null);
    }
}
