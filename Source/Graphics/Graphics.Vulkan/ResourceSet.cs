using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class ResourceSet : VulkanObject<DeviceBuffer>
{
    internal ResourceSet(VulkanResources vkRes, ref readonly ResourceSetDescription description) : base(vkRes)
    {
        byte[] descriptor = new byte[description.Layout.SizeInBytes];

        for (uint i = 0; i < description.BoundResources.Length; i++)
        {
            ulong offset = VkRes.GetExtDescriptorBuffer().GetDescriptorSetLayoutBindingOffset(VkRes.GetDevice(), description.Layout.Handle, i);

            WriteDescriptorBuffer(description.Layout.DescriptorTypes[i],
                                  description.BoundResources[i],
                                  descriptor.AsPointer(offset));
        }

        DeviceBuffer buffer = new(VkRes, description.Layout.SizeInBytes, true, description.Layout.IsLastBindless);

        VkRes.GraphicsDevice.UpdateBuffer(buffer, 0, descriptor);

        Handle = buffer;
        Layout = description.Layout;
    }

    internal override DeviceBuffer Handle { get; }

    internal ResourceLayout Layout { get; }

    public void UpdateBindless(IBindableResource[] boundResources)
    {
        if (!Layout.IsLastBindless)
        {
            throw new InvalidOperationException("Resource layout is not bindless.");
        }

        DescriptorType type = Layout.DescriptorTypes[^1];
        uint lastIdx = (uint)Layout.DescriptorTypes.Length - 1;

        byte* descriptor = (byte*)Handle.Map(Layout.SizeInBytes);
        descriptor += VkRes.GetExtDescriptorBuffer().GetDescriptorSetLayoutBindingOffset(VkRes.GetDevice(), Layout.Handle, lastIdx);

        for (uint i = 0; i < boundResources.Length; i++)
        {
            nuint descriptorSize = WriteDescriptorBuffer(type, boundResources[i], descriptor);

            descriptor += descriptorSize;
        }

        Handle.Unmap();
    }

    internal override ulong[] GetHandles()
    {
        return [];
    }

    protected override void Destroy()
    {
        Handle.Dispose();
    }

    private nuint WriteDescriptorBuffer(DescriptorType type, IBindableResource bindableResource, byte* buffer)
    {
        nuint descriptorSize;
        if (type is DescriptorType.UniformBuffer or DescriptorType.UniformBufferDynamic or DescriptorType.StorageBuffer or DescriptorType.StorageBufferDynamic)
        {
            bool isUniform = type is DescriptorType.UniformBuffer or DescriptorType.UniformBufferDynamic;

            DeviceBufferRange range = Util.GetBufferRange(bindableResource, 0);

            DescriptorAddressInfoEXT addressInfo = new()
            {
                SType = StructureType.DescriptorAddressInfoExt,
                Address = range.Buffer.Address + range.Offset,
                Range = range.SizeInBytes,
                Format = Format.Undefined
            };

            if (isUniform)
            {
                descriptorSize = VkRes.DescriptorBufferProperties.UniformBufferDescriptorSize;
                GetDescriptor(new DescriptorDataEXT() { PUniformBuffer = &addressInfo });
            }
            else
            {
                descriptorSize = VkRes.DescriptorBufferProperties.StorageBufferDescriptorSize;
                GetDescriptor(new DescriptorDataEXT() { PStorageBuffer = &addressInfo });
            }
        }
        else if (type == DescriptorType.SampledImage)
        {
            TextureView textureView = (TextureView)bindableResource;

            DescriptorImageInfo imageInfo = new()
            {
                ImageView = textureView.Handle,
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal
            };

            descriptorSize = VkRes.DescriptorBufferProperties.SampledImageDescriptorSize;
            GetDescriptor(new DescriptorDataEXT() { PSampledImage = &imageInfo });
        }
        else if (type == DescriptorType.StorageImage)
        {
            TextureView textureView = (TextureView)bindableResource;

            DescriptorImageInfo imageInfo = new()
            {
                ImageView = textureView.Handle,
                ImageLayout = ImageLayout.General
            };

            descriptorSize = VkRes.DescriptorBufferProperties.StorageImageDescriptorSize;
            GetDescriptor(new DescriptorDataEXT() { PStorageImage = &imageInfo });
        }
        else if (type == DescriptorType.Sampler)
        {
            Sampler sampler = (Sampler)bindableResource;

            VkSampler vkSampler = sampler.Handle;

            descriptorSize = VkRes.DescriptorBufferProperties.SamplerDescriptorSize;
            GetDescriptor(new DescriptorDataEXT() { PSampler = &vkSampler });
        }
        else
        {
            throw new NotSupportedException();
        }

        return descriptorSize;

        void GetDescriptor(DescriptorDataEXT descriptorData)
        {
            DescriptorGetInfoEXT getInfo = new()
            {
                SType = StructureType.DescriptorGetInfoExt,
                Type = type,
                Data = descriptorData
            };

            VkRes.GetExtDescriptorBuffer().GetDescriptor(VkRes.GetDevice(),
                                                         &getInfo,
                                                         descriptorSize,
                                                         buffer);
        }
    }
}
