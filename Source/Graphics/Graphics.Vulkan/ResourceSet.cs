using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class ResourceSet : DeviceResource
{
    private readonly ResourceLayout _layout;
    private readonly DeviceBuffer _buffer;

    internal ResourceSet(GraphicsDevice graphicsDevice, ref readonly ResourceSetDescription description) : base(graphicsDevice)
    {
        byte[] descriptor = new byte[description.Layout.SizeInBytes];

        for (uint i = 0; i < description.BoundResources.Length; i++)
        {
            ulong offset = DescriptorBufferExt.GetDescriptorSetLayoutBindingOffset(Device, description.Layout.Handle, i);

            WriteDescriptorBuffer(description.Layout.DescriptorTypes[i],
                                  description.BoundResources[i],
                                  descriptor.AsPointer(offset));
        }

        DeviceBuffer buffer = new(graphicsDevice, description.Layout.SizeInBytes, true, description.Layout.IsLastBindless);

        GraphicsDevice.UpdateBuffer(buffer, 0, descriptor);

        _layout = description.Layout;
        _buffer = buffer;
    }

    internal ulong Address => _buffer.Address;

    public void UpdateBindless(IBindableResource[] boundResources)
    {
        if (!_layout.IsLastBindless)
        {
            throw new InvalidOperationException("Resource layout is not bindless.");
        }

        DescriptorType type = _layout.DescriptorTypes[^1];
        uint lastIdx = (uint)_layout.DescriptorTypes.Length - 1;

        byte* descriptor = (byte*)_buffer.Map(_layout.SizeInBytes);
        descriptor += DescriptorBufferExt.GetDescriptorSetLayoutBindingOffset(Device, _layout.Handle, lastIdx);

        for (uint i = 0; i < boundResources.Length; i++)
        {
            nuint descriptorSize = WriteDescriptorBuffer(type, boundResources[i], descriptor);

            descriptor += descriptorSize;
        }

        _buffer.Unmap();
    }

    protected override void Destroy()
    {
        _buffer.Dispose();
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
                descriptorSize = PhysicalDevice.DescriptorBufferProperties.UniformBufferDescriptorSize;
                GetDescriptor(new DescriptorDataEXT() { PUniformBuffer = &addressInfo });
            }
            else
            {
                descriptorSize = PhysicalDevice.DescriptorBufferProperties.StorageBufferDescriptorSize;
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

            descriptorSize = PhysicalDevice.DescriptorBufferProperties.SampledImageDescriptorSize;
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

            descriptorSize = PhysicalDevice.DescriptorBufferProperties.StorageImageDescriptorSize;
            GetDescriptor(new DescriptorDataEXT() { PStorageImage = &imageInfo });
        }
        else if (type == DescriptorType.Sampler)
        {
            Sampler sampler = (Sampler)bindableResource;

            VkSampler vkSampler = sampler.Handle;

            descriptorSize = PhysicalDevice.DescriptorBufferProperties.SamplerDescriptorSize;
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

            DescriptorBufferExt.GetDescriptor(Device,
                                              &getInfo,
                                              descriptorSize,
                                              buffer);
        }
    }
}
