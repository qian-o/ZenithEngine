using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class Buffer(GraphicsContext context,
                             ref readonly BufferDescription description) : GraphicsResource(context)
{
    public BufferDescription Description { get; } = description;
}
