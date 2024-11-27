using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class BottomLevelAS(GraphicsContext context,
                                    ref readonly BottomLevelASDesc desc) : GraphicsResource(context)
{
    private BottomLevelASDesc descInternal = desc;

    public ref BottomLevelASDesc Desc => ref descInternal;
}