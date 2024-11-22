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

    public static T? TryGetExtension<T>(this Vk vk, VkInstance instance) where T : NativeExtension<Vk>
    {
        if (vk.TryGetInstanceExtension(instance, out T ext))
        {
            return ext;
        }

        return null;
    }

    public static T? TryGetExtension<T>(this Vk vk, VkInstance instance, VkDevice device) where T : NativeExtension<Vk>
    {
        if (vk.TryGetDeviceExtension(instance, device, out T ext))
        {
            return ext;
        }

        return null;
    }
}
