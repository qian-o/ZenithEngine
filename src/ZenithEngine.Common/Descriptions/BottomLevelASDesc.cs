using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct BottomLevelASDesc
{
    public AccelerationStructureGeometry[] Geometries;

    public static BottomLevelASDesc New(params AccelerationStructureGeometry[] geometries)
    {
        return new()
        {
            Geometries = geometries
        };
    }
}
