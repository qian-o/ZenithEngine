using Graphics.Engine.Descriptions;

namespace Graphics.Engine.Vulkan;

internal sealed class VKResourceFactory(Context context) : ResourceFactory(context)
{
    public override Buffer CreateBuffer(ref readonly BufferDescription description)
    {
        return new VKBuffer(Context, in description);
    }
}
