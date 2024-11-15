using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct BottomLevelASDesc
{
    public AccelerationStructureGeometry[] Geometries { get; set; }

    public static BottomLevelASDesc Default(params AccelerationStructureGeometry[] geometries)
    {
        return new()
        {
            Geometries = geometries
        };
    }
}
