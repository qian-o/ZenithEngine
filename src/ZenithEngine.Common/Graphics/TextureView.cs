using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class TextureView(GraphicsContext context,
                                  ref readonly TextureViewDesc desc) : GraphicsResource(context)
{
    public TextureViewDesc Desc { get; } = desc;
}
