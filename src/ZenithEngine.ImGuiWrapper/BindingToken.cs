using ZenithEngine.Common;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.ImGuiWrapper;

internal class BindingToken(Texture texture, ResourceSet resourceSet) : DisposableObject
{
    public Texture Texture { get; } = texture;

    public ResourceSet ResourceSet { get; } = resourceSet;

    protected override void Destroy()
    {
        ResourceSet.Dispose();
    }
}
