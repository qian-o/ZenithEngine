using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal static class VulkanExtensions
{
    public static void ThrowIfError(this VkResult result)
    {
        if (result is not VkResult.Success)
        {
            throw new Exception($"Vulkan error: [{result}].");
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

    public static VKSwapChain VK(this SwapChain swapChain)
    {
        if (swapChain is not VKSwapChain)
        {
            throw new Exception("SwapChain is not a Vulkan swap chain.");
        }

        return (VKSwapChain)swapChain;
    }

    public static VKBuffer VK(this Buffer buffer)
    {
        if (buffer is not VKBuffer)
        {
            throw new Exception("Buffer is not a Vulkan buffer.");
        }

        return (VKBuffer)buffer;
    }

    public static VKTexture VK(this Texture texture)
    {
        if (texture is not VKTexture)
        {
            throw new Exception("Texture is not a Vulkan texture.");
        }

        return (VKTexture)texture;
    }

    public static VKSampler VK(this Sampler sampler)
    {
        if (sampler is not VKSampler)
        {
            throw new Exception("Sampler is not a Vulkan sampler.");
        }

        return (VKSampler)sampler;
    }

    public static VKShader VK(this Shader shader)
    {
        if (shader is not VKShader)
        {
            throw new Exception("Shader is not a Vulkan shader.");
        }

        return (VKShader)shader;
    }

    public static VKBottomLevelAS VK(this BottomLevelAS bottomLevelAS)
    {
        if (bottomLevelAS is not VKBottomLevelAS)
        {
            throw new Exception("BottomLevelAS is not a Vulkan bottom level acceleration structure.");
        }

        return (VKBottomLevelAS)bottomLevelAS;
    }

    public static VKTopLevelAS VK(this TopLevelAS topLevelAS)
    {
        if (topLevelAS is not VKTopLevelAS)
        {
            throw new Exception("TopLevelAS is not a Vulkan top level acceleration structure.");
        }

        return (VKTopLevelAS)topLevelAS;
    }

    public static VKResourceLayout VK(this ResourceLayout resourceLayout)
    {
        if (resourceLayout is not VKResourceLayout)
        {
            throw new Exception("ResourceLayout is not a Vulkan resource layout.");
        }

        return (VKResourceLayout)resourceLayout;
    }

    public static VKResourceSet VK(this ResourceSet resourceSet)
    {
        if (resourceSet is not VKResourceSet)
        {
            throw new Exception("ResourceSet is not a Vulkan resource set.");
        }

        return (VKResourceSet)resourceSet;
    }

    public static VKFrameBuffer VK(this FrameBuffer frameBuffer)
    {
        if (frameBuffer is not VKFrameBuffer)
        {
            throw new Exception("FrameBuffer is not a Vulkan frame buffer.");
        }

        return (VKFrameBuffer)frameBuffer;
    }

    public static VKGraphicsPipeline VK(this GraphicsPipeline pipeline)
    {
        if (pipeline is not VKGraphicsPipeline)
        {
            throw new Exception("GraphicsPipeline is not a Vulkan graphics pipeline.");
        }

        return (VKGraphicsPipeline)pipeline;
    }

    public static VKComputePipeline VK(this ComputePipeline pipeline)
    {
        if (pipeline is not VKComputePipeline)
        {
            throw new Exception("ComputePipeline is not a Vulkan compute pipeline.");
        }

        return (VKComputePipeline)pipeline;
    }

    public static VKRayTracingPipeline VK(this RayTracingPipeline pipeline)
    {
        if (pipeline is not VKRayTracingPipeline)
        {
            throw new Exception("RayTracingPipeline is not a Vulkan ray tracing pipeline.");
        }

        return (VKRayTracingPipeline)pipeline;
    }

    public static VKCommandProcessor VK(this CommandProcessor processor)
    {
        if (processor is not VKCommandProcessor)
        {
            throw new Exception("CommandProcessor is not a Vulkan command processor.");
        }

        return (VKCommandProcessor)processor;
    }

    public static VKCommandBuffer VK(this CommandBuffer commandBuffer)
    {
        if (commandBuffer is not VKCommandBuffer)
        {
            throw new Exception("CommandBuffer is not a Vulkan command buffer.");
        }

        return (VKCommandBuffer)commandBuffer;
    }
}
