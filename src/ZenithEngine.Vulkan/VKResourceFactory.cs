using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal class VKResourceFactory(GraphicsContext context) : ResourceFactory(context)
{
    public override SwapChain CreateSwapChain(ref readonly SwapChainDesc desc)
    {
        throw new NotImplementedException();
    }

    public override Buffer CreateBuffer(ref readonly BufferDesc desc)
    {
        return new VKBuffer(Context, in desc);
    }

    public override Texture CreateTexture(ref readonly TextureDesc desc)
    {
        return new VKTexture(Context, in desc);
    }

    public override TextureView CreateTextureView(ref readonly TextureViewDesc desc)
    {
        return new VKTextureView(Context, in desc);
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
        throw new NotImplementedException();
    }

    public override ResourceSet CreateResourceSet(ref readonly ResourceSetDesc desc)
    {
        throw new NotImplementedException();
    }

    public override FrameBuffer CreateFrameBuffer(ref readonly FrameBufferDesc desc)
    {
        throw new NotImplementedException();
    }

    public override GraphicsPipeline CreateGraphicsPipeline(ref readonly GraphicsPipelineDesc desc)
    {
        throw new NotImplementedException();
    }

    public override ComputePipeline CreateComputePipeline(ref readonly ComputePipelineDesc desc)
    {
        throw new NotImplementedException();
    }

    public override RayTracingPipeline CreateRayTracingPipeline(ref readonly RayTracingPipelineDesc desc)
    {
        throw new NotImplementedException();
    }

    public override CommandProcessor CreateCommandProcessor(CommandProcessorType type = CommandProcessorType.Direct)
    {
        throw new NotImplementedException();
    }
}
