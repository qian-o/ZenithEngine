using ZenithEngine.Common;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.ImGuiWrapper;

internal class BindingToken(TextureView textureView, ResourceSet resourceSet) : DisposableObject
{
    public TextureView TextureView { get; } = textureView;

    public ResourceSet ResourceSet { get; } = resourceSet;

    public Texture? Texture { get; set; }

    protected override void Destroy()
    {
        if (Texture is not null)
        {
            TextureView.Dispose();
        }

        ResourceSet.Dispose();
    }
}
