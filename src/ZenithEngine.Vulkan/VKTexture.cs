using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal class VKTexture : Texture
{
    public VkImage Image;

    public VKTexture(GraphicsContext context,
                     ref readonly TextureDesc desc) : base(context, in desc)
    {
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
