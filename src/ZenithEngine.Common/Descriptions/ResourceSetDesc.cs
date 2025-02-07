using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct ResourceSetDesc(ResourceLayout layout,
                              params GraphicsResource[] resources)
{
    public ResourceSetDesc() : this(null!, [])
    {
    }

    /// <summary>
    /// The layout of the resource set.
    /// </summary>
    public ResourceLayout Layout = layout;

    /// <summary>
    /// An array of resources that are bound to the resource set.
    /// </summary>
    public GraphicsResource[] Resources = resources;
}
