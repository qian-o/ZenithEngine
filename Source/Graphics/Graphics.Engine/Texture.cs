using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class Texture(Context context,
                              ref readonly TextureDescription description) : DeviceResource(context)
{
    public TextureDescription Description { get; } = description;

    public abstract uint Width { get; }

    public abstract uint Height { get; }

    public abstract uint Depth { get; }
}
