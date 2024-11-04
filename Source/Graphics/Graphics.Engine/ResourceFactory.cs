using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class ResourceFactory(Context context)
{
    public Context Context { get; } = context;

    public abstract Shader CreateShader(ref readonly ShaderDescription description);

    public abstract Buffer CreateBuffer(ref readonly BufferDescription description);

    public abstract Texture CreateTexture(ref readonly TextureDescription description);
}
