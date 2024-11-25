using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class Sampler(GraphicsContext context,
                              ref readonly SamplerDesc desc) : GraphicsResource(context)
{
    private SamplerDesc descInternal = desc;

    public ref SamplerDesc Desc => ref descInternal;
}
