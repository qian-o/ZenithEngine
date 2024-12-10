using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Vulkan;

internal unsafe partial class VKGraphicsContext
{
    public VkPhysicalDevice PhysicalDevice;

    public uint DirectQueueFamilyIndex { get; private set; }

    public uint CopyQueueFamilyIndex { get; private set; }

    public bool SharingEnabled => DirectQueueFamilyIndex != CopyQueueFamilyIndex;

    public uint FindMemoryTypeIndex(uint typeBits, MemoryPropertyFlags flags)
    {
        PhysicalDeviceMemoryProperties properties;
        Vk.GetPhysicalDeviceMemoryProperties(PhysicalDevice, &properties);

        for (int i = 0; i < properties.MemoryTypeCount; i++)
        {
            MemoryType memoryType = properties.MemoryTypes[i];

            if ((typeBits & (1 << i)) is not 0 && memoryType.PropertyFlags.HasFlag(flags))
            {
                return (uint)i;
            }
        }

        throw new ZenithEngineException("Failed to find suitable memory type.");
    }

    public uint FindQueueFamilyIndex(CommandProcessorType type)
    {
        return type switch
        {
            CommandProcessorType.Direct => DirectQueueFamilyIndex,
            CommandProcessorType.Copy => CopyQueueFamilyIndex,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private void InitPhysicalDevice()
    {
        uint physicalDeviceCount = 0;
        Vk.EnumeratePhysicalDevices(Instance, &physicalDeviceCount, null).ThrowIfError();

        if (physicalDeviceCount is 0)
        {
            throw new ZenithEngineException("No physical devices found.");
        }

        VkPhysicalDevice[] physicalDevices = new VkPhysicalDevice[physicalDeviceCount];
        Vk.EnumeratePhysicalDevices(Instance, &physicalDeviceCount, physicalDevices).ThrowIfError();

        uint bestScore = 0;
        VkPhysicalDevice bestPhysicalDevice = default;

        foreach (VkPhysicalDevice physicalDevice in physicalDevices)
        {
            uint score = CalcPhysicalDeviceScore(physicalDevice);

            if (score > bestScore)
            {
                bestScore = score;
                bestPhysicalDevice = physicalDevice;
                (DirectQueueFamilyIndex, CopyQueueFamilyIndex) = QueueFamilyIndices(physicalDevice);
            }
        }

        PhysicalDevice = bestPhysicalDevice;

        Capabilities.Init(this);
    }

    private uint CalcPhysicalDeviceScore(VkPhysicalDevice physicalDevice)
    {
        PhysicalDeviceFeatures features;
        Vk.GetPhysicalDeviceFeatures(physicalDevice, &features);

        PhysicalDeviceProperties properties;
        Vk.GetPhysicalDeviceProperties(physicalDevice, &properties);

        uint score = 0;

        if (features.GeometryShader)
        {
            score += 100;
        }

        if (features.TessellationShader)
        {
            score += 100;
        }

        if (features.ShaderInt16)
        {
            score += 100;
        }

        if (features.ShaderInt64)
        {
            score += 100;
        }

        if (features.ShaderFloat64)
        {
            score += 100;
        }

        if (features.SparseBinding)
        {
            score += 100;
        }

        if (features.SparseResidencyBuffer)
        {
            score += 100;
        }

        if (features.SparseResidencyImage2D)
        {
            score += 100;
        }

        if (features.SparseResidencyImage3D)
        {
            score += 100;
        }

        if (features.SparseResidency2Samples)
        {
            score += 100;
        }

        if (features.SparseResidency4Samples)
        {
            score += 100;
        }

        if (features.SparseResidency8Samples)
        {
            score += 100;
        }

        if (features.SparseResidency16Samples)
        {
            score += 100;
        }

        if (features.SparseResidencyAliased)
        {
            score += 100;
        }

        if (features.VariableMultisampleRate)
        {
            score += 100;
        }

        if (features.InheritedQueries)
        {
            score += 100;
        }

        score += properties.ApiVersion;

        if (properties.DeviceType is PhysicalDeviceType.IntegratedGpu)
        {
            score += 4000;
        }

        if (properties.DeviceType is PhysicalDeviceType.DiscreteGpu)
        {
            score += 6000;
        }

        if (properties.DeviceType is PhysicalDeviceType.VirtualGpu)
        {
            score += 2000;
        }

        if (properties.DeviceType is PhysicalDeviceType.Cpu)
        {
            score += 1000;
        }

        return score;
    }

    private (uint Direct, uint Copy) QueueFamilyIndices(VkPhysicalDevice physicalDevice)
    {
        uint propertyCount = 0;
        Vk.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, &propertyCount, null);

        QueueFamilyProperties[] properties = new QueueFamilyProperties[propertyCount];
        Vk.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, &propertyCount, properties);

        uint directQueueFamilyIndex = 0;
        uint copyQueueFamilyIndex = 0;

        uint directQueueCount = 0;
        uint copyQueueCount = 0;

        for (uint i = 0; i < properties.Length; i++)
        {
            QueueFlags flags = properties[i].QueueFlags;
            uint count = properties[i].QueueCount;

            if (flags.HasFlag(QueueFlags.GraphicsBit
                              | QueueFlags.ComputeBit
                              | QueueFlags.TransferBit) && directQueueCount < count)
            {
                directQueueFamilyIndex = i;

                directQueueCount = count;
            }
            else if (flags.HasFlag(QueueFlags.TransferBit) && copyQueueCount < count)
            {
                copyQueueFamilyIndex = i;

                copyQueueCount = count;
            }
        }

        if (copyQueueFamilyIndex is 0)
        {
            copyQueueFamilyIndex = directQueueFamilyIndex;
        }

        return (directQueueFamilyIndex, copyQueueFamilyIndex);
    }
}
