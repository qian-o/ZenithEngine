using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal class DXResourceLayout(GraphicsContext context,
                                ref readonly ResourceLayoutDesc desc) : ResourceLayout(context, in desc)
{
    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
