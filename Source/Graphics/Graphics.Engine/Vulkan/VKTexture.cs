using Graphics.Engine.Descriptions;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKTexture : Texture
{
    public VKTexture(Context context, ref readonly TextureDescription description) : base(context, in description)
    {
    }

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
