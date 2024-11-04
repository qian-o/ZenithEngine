using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class Buffer(Context context,
                             ref readonly BufferDescription description) : DeviceResource(context)
{
    public BufferDescription Description { get; } = description;
}
