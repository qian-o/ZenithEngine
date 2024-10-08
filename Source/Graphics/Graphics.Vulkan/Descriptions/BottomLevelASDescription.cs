namespace Graphics.Vulkan.Descriptions;

public record struct BottomLevelASDescription
{
    public BottomLevelASDescription(params ASGeometry[] geometries)
    {
        Geometries = geometries;
    }

    public ASGeometry[] Geometries { get; set; }
}
