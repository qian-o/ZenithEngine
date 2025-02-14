using System.Runtime.InteropServices;
using ZenithEngine.Common;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal static class DirectX12Extensions
{
    public static void ThrowIfError(this int result)
    {
        if (result is not 0)
        {
            throw new ZenithEngineException($"DirectX12 error code: {result}", Marshal.GetExceptionForHR(result));
        }
    }

    public static DXSwapChain DX(this SwapChain swapChain)
    {
        if (swapChain is not DXSwapChain)
        {
            throw new ZenithEngineException("SwapChain is not a DirectX12 swap chain.");
        }

        return (DXSwapChain)swapChain;
    }

    public static DXBuffer DX(this Buffer buffer)
    {
        if (buffer is not DXBuffer)
        {
            throw new ZenithEngineException("Buffer is not a DirectX12 buffer.");
        }

        return (DXBuffer)buffer;
    }

    public static DXTexture DX(this Texture texture)
    {
        if (texture is not DXTexture)
        {
            throw new ZenithEngineException("Texture is not a DirectX12 texture.");
        }

        return (DXTexture)texture;
    }

    public static DXSampler DX(this Sampler sampler)
    {
        if (sampler is not DXSampler)
        {
            throw new ZenithEngineException("Sampler is not a DirectX12 sampler.");
        }

        return (DXSampler)sampler;
    }

    public static DXShader DX(this Shader shader)
    {
        if (shader is not DXShader)
        {
            throw new ZenithEngineException("Shader is not a DirectX12 shader.");
        }

        return (DXShader)shader;
    }

    public static DXResourceLayout DX(this ResourceLayout layout)
    {
        if (layout is not DXResourceLayout)
        {
            throw new ZenithEngineException("ResourceLayout is not a DirectX12 resource layout.");
        }

        return (DXResourceLayout)layout;
    }

    public static DXResourceSet DX(this ResourceSet set)
    {
        if (set is not DXResourceSet)
        {
            throw new ZenithEngineException("ResourceSet is not a DirectX12 resource set.");
        }

        return (DXResourceSet)set;
    }

    public static DXFrameBuffer DX(this FrameBuffer frameBuffer)
    {
        if (frameBuffer is not DXFrameBuffer)
        {
            throw new ZenithEngineException("FrameBuffer is not a DirectX12 frame buffer.");
        }

        return (DXFrameBuffer)frameBuffer;
    }

    public static DXGraphicsPipeline DX(this GraphicsPipeline pipeline)
    {
        if (pipeline is not DXGraphicsPipeline)
        {
            throw new ZenithEngineException("GraphicsPipeline is not a DirectX12 graphics pipeline.");
        }

        return (DXGraphicsPipeline)pipeline;
    }

    public static DXComputePipeline DX(this ComputePipeline pipeline)
    {
        if (pipeline is not DXComputePipeline)
        {
            throw new ZenithEngineException("ComputePipeline is not a DirectX12 compute pipeline.");
        }

        return (DXComputePipeline)pipeline;
    }

    public static DXRayTracingPipeline DX(this RayTracingPipeline pipeline)
    {
        if (pipeline is not DXRayTracingPipeline)
        {
            throw new ZenithEngineException("RayTracingPipeline is not a DirectX12 ray tracing pipeline.");
        }

        return (DXRayTracingPipeline)pipeline;
    }

    public static DXCommandProcessor DX(this CommandProcessor processor)
    {
        if (processor is not DXCommandProcessor)
        {
            throw new ZenithEngineException("CommandProcessor is not a DirectX12 command processor.");
        }

        return (DXCommandProcessor)processor;
    }

    public static DXCommandBuffer DX(this CommandBuffer buffer)
    {
        if (buffer is not DXCommandBuffer)
        {
            throw new ZenithEngineException("CommandBuffer is not a DirectX12 command buffer.");
        }

        return (DXCommandBuffer)buffer;
    }
}
