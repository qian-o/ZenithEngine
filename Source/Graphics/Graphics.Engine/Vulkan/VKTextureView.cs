using Graphics.Engine.Descriptions;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKTextureView : TextureView
{
    public VKTextureView(Context context,
                         ref readonly TextureViewDescription description) : base(context, in description)
    {
    }

    public new VKContext Context => (VKContext)base.Context;

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
