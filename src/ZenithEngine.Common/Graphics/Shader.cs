using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class Shader(GraphicsContext context,
                             ref readonly ShaderDesc desc) : GraphicsResource(context)
{
    private ShaderDesc descInternal = desc;

    public ref ShaderDesc Desc => ref descInternal;
}
