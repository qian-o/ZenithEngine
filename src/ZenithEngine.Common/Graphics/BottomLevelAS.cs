using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class BottomLevelAS(GraphicsContext context,
                                    ref readonly BottomLevelASDesc desc) : GraphicsResource(context)
{
    public BottomLevelASDesc Desc { get; } = desc;
}