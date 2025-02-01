using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal class DXResourceSet(GraphicsContext context,
                             ref readonly ResourceSetDesc desc) : ResourceSet(context, in desc)
{
    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
