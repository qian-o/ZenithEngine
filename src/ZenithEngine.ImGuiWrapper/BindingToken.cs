using ZenithEngine.Common;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.ImGuiWrapper;

internal class BindingToken(TextureView textureView, ResourceSet resourceSet) : DisposableObject
{
    public TextureView TextureView { get; } = textureView;

    public ResourceSet ResourceSet { get; } = resourceSet;

    /// <summary>
    /// If it is null, it means the current binding comes from an external TextureView.
    /// Otherwise, it means the current binding TextureView is created by ImGuiRenderer.
    /// </summary>
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
