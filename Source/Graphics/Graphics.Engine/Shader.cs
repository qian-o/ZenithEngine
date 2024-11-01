using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class Shader(GraphicsContext context,
                             ref readonly ShaderDescription description) : GraphicsResource(context)
{
    public ShaderDescription Description { get; } = description;
}
