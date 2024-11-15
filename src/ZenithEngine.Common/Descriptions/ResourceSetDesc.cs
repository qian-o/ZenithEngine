using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct ResourceSetDesc
{
    /// <summary>
    /// The layout of the resource set.
    /// </summary>
    public ResourceLayout Layout { get; set; }

    /// <summary>
    /// An array of resources that are bound to the resource set.
    /// </summary>
    public GraphicsResource[] Resources { get; set; }

    public static ResourceSetDesc Default(ResourceLayout layout,
                                          params GraphicsResource[] resources)
    {
        return new()
        {
            Layout = layout,
            Resources = resources
        };
    }
}
