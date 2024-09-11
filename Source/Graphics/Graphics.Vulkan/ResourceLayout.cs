using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class ResourceLayout : DeviceResource
{
    private readonly VkDescriptorSetLayout _descriptorSetLayout;
    private readonly DescriptorType[] _descriptorTypes;
    private readonly uint _sizeInBytes;

    internal ResourceLayout(GraphicsDevice graphicsDevice, ref readonly ResourceLayoutDescription description) : base(graphicsDevice)
    {
        DescriptorSetLayoutBinding[] bindings = new DescriptorSetLayoutBinding[description.Elements.Length];

        uint uniformBufferCount = 0;
        uint uniformBufferDynamicCount = 0;
        uint sampledImageCount = 0;
        uint samplerCount = 0;
        uint storageBufferCount = 0;
        uint storageBufferDynamicCount = 0;
        uint storageImageCount = 0;
        DescriptorType[] descriptorTypes = new DescriptorType[description.Elements.Length];

        for (uint i = 0; i < description.Elements.Length; i++)
        {
            ResourceLayoutElementDescription element = description.Elements[i];

            DescriptorSetLayoutBinding binding = new()
            {
                Binding = i,
                DescriptorType = Formats.GetDescriptorType(element.Kind, element.Options),
                DescriptorCount = 1,
                StageFlags = Formats.GetShaderStageFlags(element.Stages)
            };

            bindings[i] = binding;

            switch (binding.DescriptorType)
            {
                case DescriptorType.Sampler:
                    samplerCount++;
                    break;
                case DescriptorType.SampledImage:
                    sampledImageCount++;
                    break;
                case DescriptorType.StorageImage:
                    storageImageCount++;
                    break;
                case DescriptorType.UniformBuffer:
                    uniformBufferCount++;
                    break;
                case DescriptorType.UniformBufferDynamic:
                    uniformBufferDynamicCount++;
                    break;
                case DescriptorType.StorageBuffer:
                    storageBufferCount++;
                    break;
                case DescriptorType.StorageBufferDynamic:
                    storageBufferDynamicCount++;
                    break;
            }

            descriptorTypes[i] = binding.DescriptorType;
        }

        DescriptorSetLayoutCreateInfo createInfo = new()
        {
            SType = StructureType.DescriptorSetLayoutCreateInfo,
            BindingCount = (uint)bindings.Length,
            PBindings = bindings.AsPointer(),
            Flags = DescriptorSetLayoutCreateFlags.DescriptorBufferBitExt
        };

        VkDescriptorSetLayout descriptorSetLayout;
        Vk.CreateDescriptorSetLayout(graphicsDevice.Device, &createInfo, null, &descriptorSetLayout).ThrowCode();

        ulong sizeInBytes;
        DescriptorBufferExt.GetDescriptorSetLayoutSize(graphicsDevice.Device, descriptorSetLayout, &sizeInBytes);
        sizeInBytes = Util.AlignedSize(sizeInBytes, PhysicalDevice.DescriptorBufferProperties.DescriptorBufferOffsetAlignment);

        _descriptorSetLayout = descriptorSetLayout;
        _descriptorTypes = descriptorTypes;
        _sizeInBytes = (uint)sizeInBytes;
    }

    internal VkDescriptorSetLayout Handle => _descriptorSetLayout;

    internal DescriptorType[] DescriptorTypes => _descriptorTypes;

    internal uint SizeInBytes => _sizeInBytes;

    protected override void Destroy()
    {
        Vk.DestroyDescriptorSetLayout(GraphicsDevice.Device, _descriptorSetLayout, null);
    }
}
