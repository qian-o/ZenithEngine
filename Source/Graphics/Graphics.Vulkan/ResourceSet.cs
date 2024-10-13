using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class ResourceSet : VulkanObject<ulong>
{
    private DeviceBuffer? descriptorBuffer;

    internal ResourceSet(VulkanResources vkRes, ref readonly ResourceSetDescription description) : base(vkRes)
    {
        if (VkRes.DescriptorBufferSupported)
        {
            const BufferUsageFlags bufferUsageFlags = BufferUsageFlags.TransferDstBit
                                                      | BufferUsageFlags.ResourceDescriptorBufferBitExt
                                                      | BufferUsageFlags.SamplerDescriptorBufferBitExt;

            descriptorBuffer = new(VkRes, bufferUsageFlags, description.Layout.SizeInBytes, true);

            byte* descriptor = (byte*)descriptorBuffer.Map(description.Layout.SizeInBytes);

            for (uint i = 0; i < description.BoundResources.Length; i++)
            {
                IBindableResource? bindableResource = description.BoundResources[i];

                if (bindableResource is not null)
                {
                    ulong offset = VkRes.ExtDescriptorBuffer.GetDescriptorSetLayoutBindingOffset(VkRes.VkDevice,
                                                                                                 description.Layout.Handle,
                                                                                                 i);

                    WriteDescriptorBuffer(description.Layout.DescriptorTypes[i],
                                          bindableResource,
                                          descriptor + offset);
                }
            }

            descriptorBuffer.Unmap();

            Handle = descriptorBuffer.Address;
        }

        Layout = description.Layout;
    }

    internal override ulong Handle { get; }

    internal ResourceLayout Layout { get; }

    public void UpdateSet(IBindableResource bindableResource, uint index)
    {
        if (index >= Layout.DescriptorTypes.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (Layout.IsLastBindless && index >= Layout.DescriptorTypes.Length - 1)
        {
            throw new InvalidOperationException("Resource layout is bindless.");
        }

        if (VkRes.DescriptorBufferSupported)
        {
            DescriptorType type = Layout.DescriptorTypes[index];

            byte* descriptor = (byte*)descriptorBuffer!.Map(Layout.SizeInBytes);
            descriptor += VkRes.ExtDescriptorBuffer.GetDescriptorSetLayoutBindingOffset(VkRes.VkDevice, Layout.Handle, index);

            WriteDescriptorBuffer(type, bindableResource, descriptor);

            descriptorBuffer.Unmap();
        }
    }

    public void UpdateBindless(IBindableResource[] boundResources)
    {
        if (!Layout.IsLastBindless)
        {
            throw new InvalidOperationException("Resource layout is not bindless.");
        }

        if (VkRes.DescriptorBufferSupported)
        {
            DescriptorType type = Layout.DescriptorTypes[^1];
            uint lastIdx = (uint)Layout.DescriptorTypes.Length - 1;

            byte* descriptor = (byte*)descriptorBuffer!.Map(Layout.SizeInBytes);
            descriptor += VkRes.ExtDescriptorBuffer.GetDescriptorSetLayoutBindingOffset(VkRes.VkDevice, Layout.Handle, lastIdx);

            for (uint i = 0; i < boundResources.Length; i++)
            {
                descriptor += WriteDescriptorBuffer(type, boundResources[i], descriptor);
            }

            descriptorBuffer.Unmap();
        }
    }

    internal override ulong[] GetHandles()
    {
        return [];
    }

    protected override void Destroy()
    {
        descriptorBuffer?.Dispose();
    }

    private nuint WriteDescriptorBuffer(DescriptorType type, IBindableResource bindableResource, byte* buffer)
    {
        nuint descriptorSize;
        if (type is DescriptorType.UniformBuffer
            or DescriptorType.UniformBufferDynamic
            or DescriptorType.StorageBuffer
            or DescriptorType.StorageBufferDynamic)
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
        else if (type == DescriptorType.AccelerationStructureKhr)
        {
            TopLevelAS topLevelAS = (TopLevelAS)bindableResource;

            descriptorSize = VkRes.DescriptorBufferProperties.AccelerationStructureDescriptorSize;
            GetDescriptor(new DescriptorDataEXT() { AccelerationStructure = topLevelAS.Address });
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

            VkRes.ExtDescriptorBuffer.GetDescriptor(VkRes.VkDevice,
                                                    &getInfo,
                                                    descriptorSize,
                                                    buffer);
        }
    }
}
