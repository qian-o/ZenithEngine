using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class QueryHeap(GraphicsContext context,
                                ref readonly QueryHeapDesc desc) : GraphicsResource(context)
{
    private QueryHeapDesc descInternal = desc;

    public ref QueryHeapDesc Desc => ref descInternal;

    public abstract void GetData(int startIndex, Span<ulong> data);
}