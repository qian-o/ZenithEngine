using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public abstract class ResourceFactory(GraphicsContext context)
{
    public GraphicsContext Context { get; } = context;

    public abstract SwapChain CreateSwapChain(ref readonly SwapChainDesc desc);

    public abstract Buffer CreateBuffer(ref readonly BufferDesc desc);

    public abstract Texture CreateTexture(ref readonly TextureDesc desc);

    public abstract Sampler CreateSampler(ref readonly SamplerDesc desc);

    public abstract Shader CreateShader(ref readonly ShaderDesc desc);

    public abstract ResourceLayout CreateResourceLayout(ref readonly ResourceLayoutDesc desc);

    public abstract ResourceSet CreateResourceSet(ref readonly ResourceSetDesc desc);

    public abstract FrameBuffer CreateFrameBuffer(ref readonly FrameBufferDesc desc);

    public abstract GraphicsPipeline CreateGraphicsPipeline(ref readonly GraphicsPipelineDesc desc);

    public abstract ComputePipeline CreateComputePipeline(ref readonly ComputePipelineDesc desc);

    public abstract RayTracingPipeline CreateRayTracingPipeline(ref readonly RayTracingPipelineDesc desc);

    public abstract CommandProcessor CreateCommandProcessor(CommandProcessorType type = CommandProcessorType.Graphics);
}
