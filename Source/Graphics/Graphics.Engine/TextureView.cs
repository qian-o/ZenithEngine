using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class TextureView(Context context,
                                  ref readonly TextureViewDescription description) : DeviceResource(context)
{
    public TextureViewDescription Description { get; } = description;
}
