using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class Sampler(GraphicsContext context,
                              ref readonly SamplerDesc desc) : GraphicsResource(context)
{
    public SamplerDesc Desc { get; } = desc;
}
