using Graphics.Core.RayTracing;

namespace Graphics.Vulkan.RayTracing;

public abstract class Geometry
{
    public GeometryMask Mask { get; set; }
}
