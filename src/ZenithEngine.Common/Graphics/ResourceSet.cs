using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class ResourceSet(GraphicsContext context,
                                  ref readonly ResourceSetDesc desc) : GraphicsResource(context)
{
    public ResourceSetDesc Desc { get; } = desc;
}
