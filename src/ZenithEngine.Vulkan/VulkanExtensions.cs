using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Vulkan;

internal static class VulkanExtensions
{
    public static void ThrowIfError(this VkResult result)
    {
        if (result != VkResult.Success)
        {
            throw new ZenithEngineException(Backend.Vulkan, $"Vulkan error: {result}");
        }
    }

    public static T GetExtension<T>(this Vk vk, VkInstance instance) where T : NativeExtension<Vk>
    {
        if (!vk.TryGetInstanceExtension(instance, out T ext))
        {
            throw new InvalidOperationException($"Failed to load extension {typeof(T).Name}!");
        }

        return ext;
    }

    public static T GetExtension<T>(this Vk vk, VkInstance instance, VkDevice device) where T : NativeExtension<Vk>
    {
        if (!vk.TryGetDeviceExtension(instance, device, out T ext))
        {
            throw new InvalidOperationException($"Failed to load extension {typeof(T).Name}!");
        }

        return ext;
    }
}
