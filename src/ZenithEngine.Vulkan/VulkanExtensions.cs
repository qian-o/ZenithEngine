using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

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

    public static VKBuffer VK(this Buffer buffer)
    {
        if (buffer is not VKBuffer)
        {
            throw new ZenithEngineException(Backend.Vulkan, "Buffer is not a Vulkan buffer");
        }

        return (VKBuffer)buffer;
    }

    public static VKTexture VK(this Texture texture)
    {
        if (texture is not VKTexture)
        {
            throw new ZenithEngineException(Backend.Vulkan, "Texture is not a Vulkan texture");
        }

        return (VKTexture)texture;
    }

    public static VKTextureView VK(this TextureView textureView)
    {
        if (textureView is not VKTextureView)
        {
            throw new ZenithEngineException(Backend.Vulkan, "TextureView is not a Vulkan texture view");
        }

        return (VKTextureView)textureView;
    }
}
