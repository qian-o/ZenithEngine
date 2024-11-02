using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class ResourceFactory(Context context)
{
    public Context Context { get; } = context;

    public abstract Buffer CreateBuffer(ref readonly BufferDescription description);
}
