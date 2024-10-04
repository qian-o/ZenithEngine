namespace Graphics.Vulkan.Descriptions;

public record struct BottomLevelASDescription
{
    public BottomLevelASDescription(params AccelerationStructureGeometry[] geometries)
    {
        Geometries = geometries;
    }

    public AccelerationStructureGeometry[] Geometries { get; set; }
}
