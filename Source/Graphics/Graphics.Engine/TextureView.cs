using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class TextureView(Context context,
                                  ref readonly TextureViewDescription description) : DeviceResource(context)
{
    public TextureViewDescription Description { get; } = description;

    public abstract uint Width { get; }

    public abstract uint Height { get; }

    public abstract uint Depth { get; }
}
