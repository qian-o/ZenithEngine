using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class TextureView(GraphicsContext context,
                                  ref readonly TextureViewDesc desc) : GraphicsResource(context)
{
    private TextureViewDesc descInternal = desc;

    public ref TextureViewDesc Desc => ref descInternal;
}
