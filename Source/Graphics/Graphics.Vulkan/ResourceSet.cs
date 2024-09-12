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
            DescriptorType type = description.Layout.DescriptorTypes[i];

            int offset = (int)DescriptorBufferExt.GetDescriptorSetLayoutBindingOffset(Device, description.Layout.Handle, i);

            if (type is DescriptorType.UniformBuffer or DescriptorType.UniformBufferDynamic)
            {
                DeviceBufferRange range = Util.GetBufferRange(description.BoundResources[i], 0);

                DescriptorAddressInfoEXT addressInfo = new()
                {
                    SType = StructureType.DescriptorAddressInfoExt,
                    Address = range.Buffer.Address + range.Offset,
                    Range = range.SizeInBytes,
                    Format = Format.Undefined
                };

                DescriptorGetInfoEXT getInfo = new()
                {
                    SType = StructureType.DescriptorGetInfoExt,
                    Type = type,
                    Data = new DescriptorDataEXT
                    {
                        PUniformBuffer = &addressInfo
                    }
                };

                DescriptorBufferExt.GetDescriptor(Device,
                                                  &getInfo,
                                                  PhysicalDevice.DescriptorBufferProperties.UniformBufferDescriptorSize,
                                                  descriptor.AsPointer(offset));
            }
            else if (type is DescriptorType.StorageBuffer or DescriptorType.StorageBufferDynamic)
            {
                DeviceBufferRange range = Util.GetBufferRange(description.BoundResources[i], 0);

                DescriptorAddressInfoEXT addressInfo = new()
                {
                    SType = StructureType.DescriptorAddressInfoExt,
                    Address = range.Buffer.Address + range.Offset,
                    Range = range.SizeInBytes,
                    Format = Format.Undefined
                };

                DescriptorGetInfoEXT getInfo = new()
                {
                    SType = StructureType.DescriptorGetInfoExt,
                    Type = type,
                    Data = new DescriptorDataEXT
                    {
                        PStorageBuffer = &addressInfo
                    }
                };

                DescriptorBufferExt.GetDescriptor(Device,
                                                  &getInfo,
                                                  PhysicalDevice.DescriptorBufferProperties.StorageBufferDescriptorSize,
                                                  descriptor.AsPointer(offset));
            }
            else if (type == DescriptorType.SampledImage)
            {
                TextureView textureView = (TextureView)description.BoundResources[i];

                DescriptorImageInfo imageInfo = new()
                {
                    ImageView = textureView.Handle,
                    ImageLayout = ImageLayout.ShaderReadOnlyOptimal
                };

                DescriptorGetInfoEXT getInfo = new()
                {
                    SType = StructureType.DescriptorGetInfoExt,
                    Type = type,
                    Data = new DescriptorDataEXT
                    {
                        PSampledImage = &imageInfo
                    }
                };

                DescriptorBufferExt.GetDescriptor(Device,
                                                  &getInfo,
                                                  PhysicalDevice.DescriptorBufferProperties.SampledImageDescriptorSize,
                                                  descriptor.AsPointer(offset));
            }
            else if (type == DescriptorType.StorageImage)
            {
                TextureView textureView = (TextureView)description.BoundResources[i];

                DescriptorImageInfo imageInfo = new()
                {
                    ImageView = textureView.Handle,
                    ImageLayout = ImageLayout.General
                };

                DescriptorGetInfoEXT getInfo = new()
                {
                    SType = StructureType.DescriptorGetInfoExt,
                    Type = type,
                    Data = new DescriptorDataEXT
                    {
                        PStorageImage = &imageInfo
                    }
                };

                DescriptorBufferExt.GetDescriptor(Device,
                                                  &getInfo,
                                                  PhysicalDevice.DescriptorBufferProperties.StorageImageDescriptorSize,
                                                  descriptor.AsPointer(offset));
            }
            else if (type == DescriptorType.Sampler)
            {
                Sampler sampler = (Sampler)description.BoundResources[i];

                VkSampler vkSampler = sampler.Handle;

                DescriptorGetInfoEXT getInfo = new()
                {
                    SType = StructureType.DescriptorGetInfoExt,
                    Type = type,
                    Data = new DescriptorDataEXT
                    {
                        PSampler = &vkSampler
                    }
                };

                DescriptorBufferExt.GetDescriptor(Device,
                                                  &getInfo,
                                                  PhysicalDevice.DescriptorBufferProperties.SamplerDescriptorSize,
                                                  descriptor.AsPointer(offset));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        DeviceBuffer buffer = ResourceFactory.CreateBuffer(new BufferDescription(description.Layout.SizeInBytes,
                                                                                 true,
                                                                                 description.Layout.IsLastBindless));

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
            if (type is DescriptorType.UniformBuffer or DescriptorType.UniformBufferDynamic)
            {
                DeviceBufferRange range = Util.GetBufferRange(boundResources[i], 0);

                DescriptorAddressInfoEXT addressInfo = new()
                {
                    SType = StructureType.DescriptorAddressInfoExt,
                    Address = range.Buffer.Address + range.Offset,
                    Range = range.SizeInBytes,
                    Format = Format.Undefined
                };

                DescriptorGetInfoEXT getInfo = new()
                {
                    SType = StructureType.DescriptorGetInfoExt,
                    Type = type,
                    Data = new DescriptorDataEXT
                    {
                        PUniformBuffer = &addressInfo
                    }
                };

                DescriptorBufferExt.GetDescriptor(Device,
                                                  &getInfo,
                                                  PhysicalDevice.DescriptorBufferProperties.UniformBufferDescriptorSize,
                                                  descriptor);

                descriptor += PhysicalDevice.DescriptorBufferProperties.UniformBufferDescriptorSize;
            }
            else if (type is DescriptorType.StorageBuffer or DescriptorType.StorageBufferDynamic)
            {
                DeviceBufferRange range = Util.GetBufferRange(boundResources[i], 0);

                DescriptorAddressInfoEXT addressInfo = new()
                {
                    SType = StructureType.DescriptorAddressInfoExt,
                    Address = range.Buffer.Address + range.Offset,
                    Range = range.SizeInBytes,
                    Format = Format.Undefined
                };

                DescriptorGetInfoEXT getInfo = new()
                {
                    SType = StructureType.DescriptorGetInfoExt,
                    Type = type,
                    Data = new DescriptorDataEXT
                    {
                        PStorageBuffer = &addressInfo
                    }
                };

                DescriptorBufferExt.GetDescriptor(Device,
                                                  &getInfo,
                                                  PhysicalDevice.DescriptorBufferProperties.StorageBufferDescriptorSize,
                                                  descriptor);

                descriptor += PhysicalDevice.DescriptorBufferProperties.StorageBufferDescriptorSize;
            }
            else if (type == DescriptorType.SampledImage)
            {
                TextureView textureView = (TextureView)boundResources[i];

                DescriptorImageInfo imageInfo = new()
                {
                    ImageView = textureView.Handle,
                    ImageLayout = ImageLayout.ShaderReadOnlyOptimal
                };

                DescriptorGetInfoEXT getInfo = new()
                {
                    SType = StructureType.DescriptorGetInfoExt,
                    Type = type,
                    Data = new DescriptorDataEXT
                    {
                        PSampledImage = &imageInfo
                    }
                };

                DescriptorBufferExt.GetDescriptor(Device,
                                                  &getInfo,
                                                  PhysicalDevice.DescriptorBufferProperties.SampledImageDescriptorSize,
                                                  descriptor);

                descriptor += PhysicalDevice.DescriptorBufferProperties.SampledImageDescriptorSize;
            }
            else if (type == DescriptorType.StorageImage)
            {
                TextureView textureView = (TextureView)boundResources[i];

                DescriptorImageInfo imageInfo = new()
                {
                    ImageView = textureView.Handle,
                    ImageLayout = ImageLayout.General
                };

                DescriptorGetInfoEXT getInfo = new()
                {
                    SType = StructureType.DescriptorGetInfoExt,
                    Type = type,
                    Data = new DescriptorDataEXT
                    {
                        PStorageImage = &imageInfo
                    }
                };

                DescriptorBufferExt.GetDescriptor(Device,
                                                  &getInfo,
                                                  PhysicalDevice.DescriptorBufferProperties.StorageImageDescriptorSize,
                                                  descriptor);

                descriptor += PhysicalDevice.DescriptorBufferProperties.StorageImageDescriptorSize;
            }
            else if (type == DescriptorType.Sampler)
            {
                Sampler sampler = (Sampler)boundResources[i];

                VkSampler vkSampler = sampler.Handle;

                DescriptorGetInfoEXT getInfo = new()
                {
                    SType = StructureType.DescriptorGetInfoExt,
                    Type = type,
                    Data = new DescriptorDataEXT
                    {
                        PSampler = &vkSampler
                    }
                };

                DescriptorBufferExt.GetDescriptor(Device,
                                                  &getInfo,
                                                  PhysicalDevice.DescriptorBufferProperties.SamplerDescriptorSize,
                                                  descriptor);

                descriptor += PhysicalDevice.DescriptorBufferProperties.SamplerDescriptorSize;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        _buffer.Unmap();
    }

    protected override void Destroy()
    {
        _buffer.Dispose();
    }
}
