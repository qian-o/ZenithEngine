using Graphics.Engine.Descriptions;

namespace Graphics.Engine.Vulkan;

internal sealed class VKResourceFactory(Context context) : ResourceFactory(context)
{
    public override Shader CreateShader(ref readonly ShaderDescription description)
    {
        return new VKShader(Context, in description);
    }

    public override Buffer CreateBuffer(ref readonly BufferDescription description)
    {
        return new VKBuffer(Context, in description);
    }

    public override Texture CreateTexture(ref readonly TextureDescription description)
    {
        return new VKTexture(Context, in description);
    }

    public override TextureView CreateTextureView(ref readonly TextureViewDescription description)
    {
        return new VKTextureView(Context, in description);
    }

    public override Sampler CreateSampler(ref readonly SamplerDescription description)
    {
        return new VKSampler(Context, in description);
    }
}
