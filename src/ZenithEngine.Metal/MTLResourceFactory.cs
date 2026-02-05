using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLResourceFactory(GraphicsContext graphicsContext) : ResourceFactory(graphicsContext)
{
    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public override SwapChain CreateSwapChain(ref readonly SwapChainDesc desc)
    {
        return new MTLSwapChain(Context, in desc);
    }

    public override Buffer CreateBuffer(ref readonly BufferDesc desc)
    {
        return new MTLBuffer(Context, in desc);
    }

    public override Texture CreateTexture(ref readonly TextureDesc desc)
    {
        return new MTLTexture(Context, in desc);
    }

    public override Sampler CreateSampler(ref readonly SamplerDesc desc)
    {
        return new MTLSampler(Context, in desc);
    }

    public override Shader CreateShader(ref readonly ShaderDesc desc)
    {
        return new MTLShader(Context, in desc);
    }

    public override ResourceLayout CreateResourceLayout(ref readonly ResourceLayoutDesc desc)
    {
        return new MTLResourceLayout(Context, in desc);
    }

    public override ResourceSet CreateResourceSet(ref readonly ResourceSetDesc desc)
    {
        return new MTLResourceSet(Context, in desc);
    }

    public override FrameBuffer CreateFrameBuffer(ref readonly FrameBufferDesc desc)
    {
        return new MTLFrameBuffer(Context, in desc);
    }

    public override GraphicsPipeline CreateGraphicsPipeline(ref readonly GraphicsPipelineDesc desc)
    {
        return new MTLGraphicsPipeline(Context, in desc);
    }

    public override ComputePipeline CreateComputePipeline(ref readonly ComputePipelineDesc desc)
    {
        return new MTLComputePipeline(Context, in desc);
    }

    public override RayTracingPipeline CreateRayTracingPipeline(ref readonly RayTracingPipelineDesc desc)
    {
        throw new NotSupportedException("Ray tracing pipelines are not implemented in the Metal backend");
    }

    public override MeshShaderPipeline CreateMeshShaderPipeline(ref readonly MeshShaderPipelineDesc desc)
    {
        return new MTLMeshShaderPipeline(Context, in desc);
    }

    public override QueryHeap CreateQueryHeap(ref readonly QueryHeapDesc desc)
    {
        return new MTLQueryHeap(Context, in desc);
    }

    public override CommandProcessor CreateCommandProcessor(CommandProcessorType type)
    {
        return new MTLCommandProcessor(Context, type);
    }
}
