using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class ResourceSet : DeviceResource
{
    private readonly DeviceBuffer _buffer;

    internal ResourceSet(GraphicsDevice graphicsDevice, ref readonly ResourceSetDescription description) : base(graphicsDevice)
    {
        byte[] descriptor = new byte[description.Layout.SizeInBytes];

        for (int i = 0; i < description.BoundResources.Length; i++)
        {
            DescriptorType type = description.Layout.DescriptorTypes[i];

            int offset = (int)DescriptorBufferExt.GetDescriptorSetLayoutBindingOffset(Device, description.Layout.Handle, (uint)i);

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

        DeviceBuffer buffer = ResourceFactory.CreateBuffer(new BufferDescription(description.Layout.SizeInBytes, true));

        GraphicsDevice.UpdateBuffer(buffer, 0, descriptor);

        _buffer = buffer;
    }

    internal ulong Address => _buffer.Address;

    protected override void Destroy()
    {
        _buffer.Dispose();
    }
}
