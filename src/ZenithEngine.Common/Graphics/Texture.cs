using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class Texture(GraphicsContext context,
                              ref readonly TextureDesc desc) : GraphicsResource(context)
{
    public TextureDesc Desc { get; } = desc;
}
