using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class ResourceLayout(GraphicsContext context,
                                     ref readonly ResourceLayoutDesc desc) : GraphicsResource(context)
{
    private ResourceLayoutDesc descInternal = desc;

    public ref ResourceLayoutDesc Desc => ref descInternal;
}

