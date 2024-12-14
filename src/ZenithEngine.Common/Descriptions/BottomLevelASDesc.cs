using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct BottomLevelASDesc
{
    public AccelerationStructureGeometry[] Geometries;

    public static BottomLevelASDesc Default(params AccelerationStructureGeometry[] geometries)
    {
        return new()
        {
            Geometries = geometries
        };
    }
}
