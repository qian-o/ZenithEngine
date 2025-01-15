using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal class VKResourceFactory(GraphicsContext context) : ResourceFactory(context)
{
    public override SwapChain CreateSwapChain(ref readonly SwapChainDesc desc)
    {
        return new VKSwapChain(Context, in desc);
    }

    public override Buffer CreateBuffer(ref readonly BufferDesc desc)
    {
        return new VKBuffer(Context, in desc);
    }

    public override Texture CreateTexture(ref readonly TextureDesc desc)
    {
        return new VKTexture(Context, in desc);
    }

    public override Sampler CreateSampler(ref readonly SamplerDesc desc)
    {
        return new VKSampler(Context, in desc);
    }

    public override Shader CreateShader(ref readonly ShaderDesc desc)
    {
        return new VKShader(Context, in desc);
    }

    public override ResourceLayout CreateResourceLayout(ref readonly ResourceLayoutDesc desc)
    {
        return new VKResourceLayout(Context, in desc);
    }

    public override ResourceSet CreateResourceSet(ref readonly ResourceSetDesc desc)
    {
        return new VKResourceSet(Context, in desc);
    }

    public override FrameBuffer CreateFrameBuffer(ref readonly FrameBufferDesc desc)
    {
        return new VKFrameBuffer(Context, in desc);
    }

    public override GraphicsPipeline CreateGraphicsPipeline(ref readonly GraphicsPipelineDesc desc)
    {
        return new VKGraphicsPipeline(Context, in desc);
    }

    public override ComputePipeline CreateComputePipeline(ref readonly ComputePipelineDesc desc)
    {
        return new VKComputePipeline(Context, in desc);
    }

    public override RayTracingPipeline CreateRayTracingPipeline(ref readonly RayTracingPipelineDesc desc)
    {
        return new VKRayTracingPipeline(Context, in desc);
    }

    public override CommandProcessor CreateCommandProcessor(CommandProcessorType type)
    {
        return new VKCommandProcessor(Context, type);
    }
}
