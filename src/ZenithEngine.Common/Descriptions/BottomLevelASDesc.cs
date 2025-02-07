using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct BottomLevelASDesc(params AccelerationStructureGeometry[] geometries)
{
    public BottomLevelASDesc()
    {
    }

    public AccelerationStructureGeometry[] Geometries = geometries;
}
