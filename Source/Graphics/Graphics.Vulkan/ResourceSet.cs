using Graphics.Core.Helpers;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class ResourceSet : VulkanObject<ulong>
{
    private readonly DeviceBuffer? descriptorBuffer;
    private readonly VkDescriptorPool descriptorPool;
    private readonly VkDescriptorSet descriptorSet;
    private readonly IBindableResource[]? useResources;

    private IBindableResource[]? bindlessResources;

    internal ResourceSet(VulkanResources vkRes, ref readonly ResourceSetDescription description) : base(vkRes)
    {
        Layout = description.Layout;

        if (VkRes.DescriptorBufferSupported)
        {
            const BufferUsageFlags bufferUsageFlags = BufferUsageFlags.TransferDstBit
                                                      | BufferUsageFlags.ResourceDescriptorBufferBitExt
                                                      | BufferUsageFlags.SamplerDescriptorBufferBitExt;

            descriptorBuffer = new(VkRes, bufferUsageFlags, Layout.SizeInBytes, true);

            byte* descriptor = (byte*)descriptorBuffer.Map(Layout.SizeInBytes);

            for (uint i = 0; i < description.BoundResources.Length; i++)
            {
                IBindableResource? bindableResource = description.BoundResources[i];

                if (bindableResource is not null)
                {
                    ulong offset = VkRes.ExtDescriptorBuffer.GetDescriptorSetLayoutBindingOffset(VkRes.VkDevice,
                                                                                                 Layout.Handle,
                                                                                                 i);

                    WriteDescriptorBuffer(Layout.DescriptorTypes[i],
                                          bindableResource,
                                          descriptor + offset);
                }
            }

            descriptorBuffer.Unmap();

            Handle = descriptorBuffer.Address;
        }
        else
        {
            DescriptorPoolSize[] poolSizes = new DescriptorPoolSize[Layout.DescriptorTypes.Length];

            for (uint i = 0; i < Layout.DescriptorTypes.Length; i++)
            {
                DescriptorType type = Layout.DescriptorTypes[i];

                DescriptorPoolSize poolSize = new()
                {
                    Type = type,
                    DescriptorCount = 1
                };

                poolSizes[i] = poolSize;
            }

            if (Layout.IsLastBindless)
            {
                poolSizes[^1].DescriptorCount = Layout.MaxDescriptorCount;
            }

            DescriptorPoolCreateInfo poolCreateInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                MaxSets = 1,
                PoolSizeCount = (uint)poolSizes.Length,
                PPoolSizes = poolSizes.AsPointer(),
                Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit
            };

            VkRes.Vk.CreateDescriptorPool(VkRes.VkDevice, &poolCreateInfo, null, out descriptorPool).ThrowCode();

            VkDescriptorSetLayout descriptorSetLayout = Layout.Handle;

            DescriptorSetAllocateInfo allocateInfo = new()
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = descriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = &descriptorSetLayout,
            };

            if (Layout.IsLastBindless)
            {
                uint[] variableDesciptorCounts = [Layout.MaxDescriptorCount];

                allocateInfo.AddNext(out DescriptorSetVariableDescriptorCountAllocateInfo variableDescriptorCountAllocateInfo);

                variableDescriptorCountAllocateInfo.DescriptorSetCount = (uint)variableDesciptorCounts.Length;
                variableDescriptorCountAllocateInfo.PDescriptorCounts = VkRes.Alloter.Allocate(variableDesciptorCounts);
            }

            VkRes.Vk.AllocateDescriptorSets(VkRes.VkDevice, &allocateInfo, out descriptorSet).ThrowCode();

            int resourceCount = Layout.IsLastBindless ? Layout.DescriptorTypes.Length - 1 : Layout.DescriptorTypes.Length;

            useResources = new IBindableResource[resourceCount];

            for (uint i = 0; i < description.BoundResources.Length; i++)
            {
                IBindableResource? bindableResource = description.BoundResources[i];

                if (bindableResource is not null)
                {
                    useResources[i] = bindableResource;
                }
            }

            RefreshDescriptorSets();

            Handle = descriptorSet.Handle;
        }
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
        else
        {
            useResources![index] = bindableResource;

            RefreshDescriptorSets();
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
        else
        {
            bindlessResources = boundResources;

            RefreshDescriptorSets();
        }
    }

    internal override ulong[] GetHandles()
    {
        return [];
    }

    protected override void Destroy()
    {
        if (VkRes.DescriptorBufferSupported)
        {
            descriptorBuffer?.Dispose();
        }
        else
        {
            VkRes.Vk.FreeDescriptorSets(VkRes.VkDevice, descriptorPool, 1, in descriptorSet);

            VkRes.Vk.DestroyDescriptorPool(VkRes.VkDevice, descriptorPool, null);
        }
    }

    private nuint WriteDescriptorBuffer(DescriptorType type, IBindableResource bindableResource, byte* buffer)
    {
        nuint descriptorSize;
        if (IsDescriptorBuffer(type))
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
        else if (IsDescriptorImage(type))
        {
            bool isSampled = type == DescriptorType.SampledImage;

            TextureView textureView = (TextureView)bindableResource;

            DescriptorImageInfo imageInfo = new()
            {
                ImageView = textureView.Handle,
                ImageLayout = isSampled ? ImageLayout.ShaderReadOnlyOptimal : ImageLayout.General
            };

            if (isSampled)
            {
                descriptorSize = VkRes.DescriptorBufferProperties.SampledImageDescriptorSize;
                GetDescriptor(new DescriptorDataEXT() { PSampledImage = &imageInfo });
            }
            else
            {
                descriptorSize = VkRes.DescriptorBufferProperties.StorageImageDescriptorSize;
                GetDescriptor(new DescriptorDataEXT() { PStorageImage = &imageInfo });
            }

        }
        else if (IsDescriptorSampler(type))
        {
            Sampler sampler = (Sampler)bindableResource;

            VkSampler vkSampler = sampler.Handle;

            descriptorSize = VkRes.DescriptorBufferProperties.SamplerDescriptorSize;
            GetDescriptor(new DescriptorDataEXT() { PSampler = &vkSampler });
        }
        else if (IsDescriptorAccelerationStructure(type))
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

    private void RefreshDescriptorSets()
    {
        if (useResources!.Any(item => item is null) || (Layout.IsLastBindless && bindlessResources is null))
        {
            return;
        }

        WriteDescriptorSet[] writeDescriptorSets = new WriteDescriptorSet[Layout.DescriptorTypes.Length];

        for (int i = 0; i < useResources!.Length; i++)
        {
            writeDescriptorSets[i] = GetWriteDescriptorSet(i, Layout.DescriptorTypes[i], useResources[i]);
        }

        if (bindlessResources != null)
        {
            writeDescriptorSets[^1] = GetWriteDescriptorSet(Layout.DescriptorTypes.Length - 1, Layout.DescriptorTypes[^1], bindlessResources);
        }

        VkRes.Vk.UpdateDescriptorSets(VkRes.VkDevice,
                                      (uint)writeDescriptorSets.Length,
                                      writeDescriptorSets.AsPointer(),
                                      0,
                                      null);
    }

    private WriteDescriptorSet GetWriteDescriptorSet(int binding, DescriptorType type, params IBindableResource[] bindableResources)
    {
        WriteDescriptorSet writeDescriptorSet = new()
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = descriptorSet,
            DstBinding = (uint)binding,
            DescriptorCount = (uint)bindableResources.Length,
            DescriptorType = type
        };

        if (IsDescriptorBuffer(type))
        {
            DescriptorBufferInfo[] bufferInfos = new DescriptorBufferInfo[bindableResources.Length];

            for (int i = 0; i < bindableResources.Length; i++)
            {
                DeviceBufferRange range = Util.GetBufferRange(bindableResources[i], 0);

                bufferInfos[i] = new()
                {
                    Buffer = range.Buffer.Handle,
                    Offset = range.Offset,
                    Range = range.SizeInBytes
                };
            }

            writeDescriptorSet.PBufferInfo = VkRes.Alloter.Allocate(bufferInfos);
        }
        else if (IsDescriptorImage(type))
        {
            bool isSampled = type == DescriptorType.SampledImage;

            DescriptorImageInfo[] imageInfos = new DescriptorImageInfo[bindableResources.Length];

            for (int i = 0; i < bindableResources.Length; i++)
            {
                TextureView textureView = (TextureView)bindableResources[i];

                imageInfos[i] = new()
                {
                    ImageView = textureView.Handle,
                    ImageLayout = isSampled ? ImageLayout.ShaderReadOnlyOptimal : ImageLayout.General
                };
            }

            writeDescriptorSet.PImageInfo = VkRes.Alloter.Allocate(imageInfos);
        }
        else if (IsDescriptorSampler(type))
        {
            DescriptorImageInfo[] imageInfos = new DescriptorImageInfo[bindableResources.Length];

            for (int i = 0; i < bindableResources.Length; i++)
            {
                Sampler sampler = (Sampler)bindableResources[i];

                imageInfos[i] = new()
                {
                    Sampler = sampler.Handle
                };
            }

            writeDescriptorSet.PImageInfo = VkRes.Alloter.Allocate(imageInfos);
        }
        else if (IsDescriptorAccelerationStructure(type))
        {
            WriteDescriptorSetAccelerationStructureKHR writeDescriptorSetAccelerationStructure = new()
            {
                SType = StructureType.WriteDescriptorSetAccelerationStructureKhr,
                AccelerationStructureCount = (uint)bindableResources.Length
            };

            AccelerationStructureKHR[] accelerationStructures = new AccelerationStructureKHR[bindableResources.Length];

            for (int i = 0; i < bindableResources.Length; i++)
            {
                TopLevelAS topLevelAS = (TopLevelAS)bindableResources[i];

                accelerationStructures[i] = topLevelAS.Handle;
            }

            writeDescriptorSetAccelerationStructure.PAccelerationStructures = VkRes.Alloter.Allocate(accelerationStructures);

            writeDescriptorSet.PNext = VkRes.Alloter.Allocate(writeDescriptorSetAccelerationStructure);
        }
        else
        {
            throw new NotSupportedException();
        }

        return writeDescriptorSet;
    }

    private static bool IsDescriptorBuffer(DescriptorType type)
    {
        return type is DescriptorType.UniformBuffer
               or DescriptorType.UniformBufferDynamic
               or DescriptorType.StorageBuffer
               or DescriptorType.StorageBufferDynamic;
    }

    private static bool IsDescriptorImage(DescriptorType type)
    {
        return type is DescriptorType.SampledImage
               or DescriptorType.StorageImage;
    }

    private static bool IsDescriptorSampler(DescriptorType type)
    {
        return type == DescriptorType.Sampler;
    }

    private static bool IsDescriptorAccelerationStructure(DescriptorType type)
    {
        return type == DescriptorType.AccelerationStructureKhr;
    }
}
