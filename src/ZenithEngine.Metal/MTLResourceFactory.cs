using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLResourceFactory(MTLGraphicsContext graphicsContext) : ResourceFactory(graphicsContext)
{
    public override Buffer CreateBuffer(ref readonly BufferDesc desc)
    {
        return new MTLBuffer(graphicsContext, in desc);
    }

    public override CommandProcessor CreateCommandProcessor(CommandProcessorType type)
    {
        throw new NotImplementedException();
    }

    public override ComputePipeline CreateComputePipeline(ref readonly ComputePipelineDesc desc)
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

    public override MeshShaderPipeline CreateMeshShaderPipeline(ref readonly MeshShaderPipelineDesc desc)
    {
        throw new NotImplementedException();
    }

    public override QueryHeap CreateQueryHeap(ref readonly QueryHeapDesc desc)
    {
        throw new NotImplementedException();
    }

    public override RayTracingPipeline CreateRayTracingPipeline(ref readonly RayTracingPipelineDesc desc)
    {
        throw new NotImplementedException();
    }

    public override ResourceLayout CreateResourceLayout(ref readonly ResourceLayoutDesc desc)
    {
        throw new NotImplementedException();
    }

    public override ResourceSet CreateResourceSet(ref readonly ResourceSetDesc desc)
    {
        throw new NotImplementedException();
    }

    public override Sampler CreateSampler(ref readonly SamplerDesc desc)
    {
        throw new NotImplementedException();
    }

    public override Shader CreateShader(ref readonly ShaderDesc desc)
    {
        throw new NotImplementedException();
    }

    public override SwapChain CreateSwapChain(ref readonly SwapChainDesc desc)
    {
        throw new NotImplementedException();
    }

    public override Texture CreateTexture(ref readonly TextureDesc desc)
    {
        throw new NotImplementedException();
    }
}
