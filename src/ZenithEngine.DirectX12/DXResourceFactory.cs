using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal class DXResourceFactory(GraphicsContext context) : ResourceFactory(context)
{
    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public override SwapChain CreateSwapChain(ref readonly SwapChainDesc desc)
    {
        return new DXSwapChain(Context, in desc);
    }

    public override Buffer CreateBuffer(ref readonly BufferDesc desc)
    {
        return new DXBuffer(Context, in desc);
    }

    public override Texture CreateTexture(ref readonly TextureDesc desc)
    {
        return new DXTexture(Context, in desc);
    }

    public override Sampler CreateSampler(ref readonly SamplerDesc desc)
    {
        return new DXSampler(Context, in desc);
    }

    public override Shader CreateShader(ref readonly ShaderDesc desc)
    {
        return new DXShader(Context, in desc);
    }

    public override ResourceLayout CreateResourceLayout(ref readonly ResourceLayoutDesc desc)
    {
        return new DXResourceLayout(Context, in desc);
    }

    public override ResourceSet CreateResourceSet(ref readonly ResourceSetDesc desc)
    {
        return new DXResourceSet(Context, in desc);
    }

    public override FrameBuffer CreateFrameBuffer(ref readonly FrameBufferDesc desc)
    {
        return new DXFrameBuffer(Context, in desc);
    }

    public override GraphicsPipeline CreateGraphicsPipeline(ref readonly GraphicsPipelineDesc desc)
    {
        return new DXGraphicsPipeline(Context, in desc);
    }

    public override ComputePipeline CreateComputePipeline(ref readonly ComputePipelineDesc desc)
    {
        throw new NotImplementedException();
    }

    public override RayTracingPipeline CreateRayTracingPipeline(ref readonly RayTracingPipelineDesc desc)
    {
        throw new NotImplementedException();
    }

    public override CommandProcessor CreateCommandProcessor(CommandProcessorType type)
    {
        if (type is CommandProcessorType.Graphics)
        {
            return Context.DefaultGraphicsCommandProcessor!;
        }

        return new DXCommandProcessor(Context, type);
    }
}
