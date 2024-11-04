using Graphics.Engine.Exceptions;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan.Helpers;

internal static class VulkanExtensions
{
    #region VulkanObject
    public static VKShader VK(this Shader shader)
    {
        if (shader is not VKShader vkShader)
        {
            throw new BackendException("Shader is not a Vulkan shader!");
        }

        return vkShader;
    }

    public static VKBuffer VK(this Buffer buffer)
    {
        if (buffer is not VKBuffer vkBuffer)
        {
            throw new BackendException("Buffer is not a Vulkan buffer!");
        }

        return vkBuffer;
    }

    public static VKTexture VK(this Texture texture)
    {
        if (texture is not VKTexture vkTexture)
        {
            throw new BackendException("Texture is not a Vulkan texture!");
        }

        return vkTexture;
    }

    public static VKTextureView VK(this TextureView textureView)
    {
        if (textureView is not VKTextureView vkTextureView)
        {
            throw new BackendException("Texture view is not a Vulkan texture view!");
        }

        return vkTextureView;
    }
    #endregion

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
