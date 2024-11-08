using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class ResourceFactory(Context context)
{
    public Context Context { get; } = context;

    public abstract Shader CreateShader(ref readonly ShaderDesc desc);

    public abstract Buffer CreateBuffer(ref readonly BufferDesc desc);

    public abstract Texture CreateTexture(ref readonly TextureDesc desc);

    public abstract TextureView CreateTextureView(ref readonly TextureViewDesc desc);

    public abstract Sampler CreateSampler(ref readonly SamplerDesc desc);

    public abstract ResourceLayout CreateResourceLayout(ref readonly ResourceLayoutDesc desc);

    public abstract FrameBuffer CreateFrameBuffer(ref readonly FrameBufferDesc desc);

    public abstract GraphicsPipeline CreateGraphicsPipeline(ref readonly GraphicsPipelineDesc desc);
}
