using Graphics.Vulkan.RayTracing;

namespace Graphics.Vulkan.Descriptions;

public record struct BottomLevelASDescription
{
    public BottomLevelASDescription(params Geometry[] geometries)
    {
        Geometries = geometries;
    }

    public Geometry[] Geometries { get; set; }
}
