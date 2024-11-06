using Graphics.Engine.Descriptions;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKFrameBuffer : FrameBuffer
{
    public VKFrameBuffer(Context context, ref readonly FrameBufferDescription description) : base(context, in description)
    {
    }

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
