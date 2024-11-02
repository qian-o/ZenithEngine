using Graphics.Engine.Exceptions;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan.Helpers;

internal static class VulkanExtensions
{
    public static void ThrowCode(this VkResult result, string message = "")
    {
        if (result != VkResult.Success)
        {
            throw new BackendException($"Vulkan error: {result} {message}");
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
