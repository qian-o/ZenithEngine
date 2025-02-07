using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct ResourceSetDesc
{
    /// <summary>
    /// The layout of the resource set.
    /// </summary>
    public ResourceLayout Layout;

    /// <summary>
    /// An array of resources that are bound to the resource set.
    /// </summary>
    public GraphicsResource[] Resources;

    public static ResourceSetDesc New(ResourceLayout layout,
                                      params GraphicsResource[] resources)
    {
        return new()
        {
            Layout = layout,
            Resources = resources
        };
    }
}
