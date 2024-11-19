using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class TopLevelAS(GraphicsContext context,
                                 ref readonly TopLevelASDesc desc) : GraphicsResource(context)
{
    public TopLevelASDesc Desc { get; } = desc;
}
