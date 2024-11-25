using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class Buffer(GraphicsContext context,
                             ref readonly BufferDesc desc) : GraphicsResource(context)
{
    private BufferDesc descInternal = desc;

    public ref BufferDesc Desc => ref descInternal;
}
