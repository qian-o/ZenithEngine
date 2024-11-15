using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common;

public abstract class Buffer(GraphicsContext context,
                             ref readonly BufferDesc desc) : GraphicsResource(context)
{
    public BufferDesc Desc { get; } = desc;
}
