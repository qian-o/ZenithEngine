using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class FrameBuffer(Context context,
                                  ref readonly FrameBufferDescription description) : DeviceResource(context)
{
    public FrameBufferDescription Description { get; } = description;
}
