using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLResourceLayout : ResourceLayout
{
    public MTLResourceLayout(GraphicsContext context,
                             ref readonly ResourceLayoutDesc desc) : base(context, in desc)
    {
        // In Metal, resource layouts are handled through argument buffers
        // The layout descriptor defines the structure of resources that will be bound
        
        // For now, we just store the descriptor
        // Actual argument buffer encoding will happen in MTLResourceSet
    }

    protected override void SetName(string name)
    {
        // Resource layouts don't have a direct Metal object to name
    }

    protected override void Destroy()
    {
        // No Metal objects to dispose
    }
}
