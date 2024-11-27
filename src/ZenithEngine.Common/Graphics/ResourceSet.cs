using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class ResourceSet(GraphicsContext context,
                                  ref readonly ResourceSetDesc desc) : GraphicsResource(context)
{
    private ResourceSetDesc descInternal = desc;

    public ref ResourceSetDesc Desc => ref descInternal;
}
