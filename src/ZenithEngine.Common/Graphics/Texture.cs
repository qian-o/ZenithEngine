using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class Texture(GraphicsContext context,
                              ref readonly TextureDesc desc) : GraphicsResource(context)
{
    private TextureDesc descInternal = desc;

    public ref TextureDesc Desc => ref descInternal;
}
