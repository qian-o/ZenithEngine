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

    public static VKSampler VK(this Sampler sampler)
    {
        if (sampler is not VKSampler)
        {
            throw new ZenithEngineException(Backend.Vulkan, "Sampler is not a Vulkan sampler");
        }

        return (VKSampler)sampler;
    }

    public static VKShader VK(this Shader shader)
    {
        if (shader is not VKShader)
        {
            throw new ZenithEngineException(Backend.Vulkan, "Shader is not a Vulkan shader");
        }

        return (VKShader)shader;
    }

    public static VKResourceLayout VK(this ResourceLayout resourceLayout)
    {
        if (resourceLayout is not VKResourceLayout)
        {
            throw new ZenithEngineException(Backend.Vulkan, "ResourceLayout is not a Vulkan resource layout");
        }

        return (VKResourceLayout)resourceLayout;
    }

    public static VKResourceSet VK(this ResourceSet resourceSet)
    {
        if (resourceSet is not VKResourceSet)
        {
            throw new ZenithEngineException(Backend.Vulkan, "ResourceSet is not a Vulkan resource set");
        }

        return (VKResourceSet)resourceSet;
    }

    public static VKFrameBuffer VK(this FrameBuffer frameBuffer)
    {
        if (frameBuffer is not VKFrameBuffer)
        {
            throw new ZenithEngineException(Backend.Vulkan, "FrameBuffer is not a Vulkan frame buffer");
        }

        return (VKFrameBuffer)frameBuffer;
    }

    public static VKGraphicsPipeline VK(this GraphicsPipeline pipeline)
    {
        if (pipeline is not VKGraphicsPipeline)
        {
            throw new ZenithEngineException(Backend.Vulkan, "GraphicsPipeline is not a Vulkan graphics pipeline");
        }

        return (VKGraphicsPipeline)pipeline;
    }

    public static VKComputePipeline VK(this ComputePipeline pipeline)
    {
        if (pipeline is not VKComputePipeline)
        {
            throw new ZenithEngineException(Backend.Vulkan, "ComputePipeline is not a Vulkan compute pipeline");
        }

        return (VKComputePipeline)pipeline;
    }
}
