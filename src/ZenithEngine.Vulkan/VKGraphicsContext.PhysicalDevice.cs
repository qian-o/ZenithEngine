using Silk.NET.Vulkan;
using ZenithEngine.Common;

namespace ZenithEngine.Vulkan;

internal unsafe partial class VKGraphicsContext
{
    public VkPhysicalDevice PhysicalDevice;

    public uint FindMemoryTypeIndex(uint typeBits, MemoryPropertyFlags flags)
    {
        PhysicalDeviceMemoryProperties properties;
        Vk.GetPhysicalDeviceMemoryProperties(PhysicalDevice, &properties);

        for (int i = 0; i < properties.MemoryTypeCount; i++)
        {
            MemoryType memoryType = properties.MemoryTypes[i];

            if ((typeBits & (1 << i)) != 0 && memoryType.PropertyFlags.HasFlag(flags))
            {
                return (uint)i;
            }
        }

        throw new ZenithEngineException("Failed to find suitable memory type.");
    }

    private void InitPhysicalDevice()
    {
        uint physicalDeviceCount = 0;
        Vk.EnumeratePhysicalDevices(Instance, &physicalDeviceCount, null).ThrowIfError();

        if (physicalDeviceCount == 0)
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
            score += 1000;
        }

        if (features.TessellationShader)
        {
            score += 1000;
        }

        if (features.ShaderInt16)
        {
            score += 1000;
        }

        if (features.ShaderInt64)
        {
            score += 1000;
        }

        if (features.ShaderFloat64)
        {
            score += 1000;
        }

        if (features.SparseBinding)
        {
            score += 1000;
        }

        if (features.SparseResidencyBuffer)
        {
            score += 1000;
        }

        if (features.SparseResidencyImage2D)
        {
            score += 1000;
        }

        if (features.SparseResidencyImage3D)
        {
            score += 1000;
        }

        if (features.SparseResidency2Samples)
        {
            score += 1000;
        }

        if (features.SparseResidency4Samples)
        {
            score += 1000;
        }

        if (features.SparseResidency8Samples)
        {
            score += 1000;
        }

        if (features.SparseResidency16Samples)
        {
            score += 1000;
        }

        if (features.SparseResidencyAliased)
        {
            score += 1000;
        }

        if (features.VariableMultisampleRate)
        {
            score += 1000;
        }

        if (features.InheritedQueries)
        {
            score += 1000;
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
}
