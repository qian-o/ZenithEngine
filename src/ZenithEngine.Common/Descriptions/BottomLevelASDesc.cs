using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct BottomLevelASDesc(params AccelerationStructureGeometry[] geometries)
{
    public BottomLevelASDesc() : this([])
    {
    }

    public AccelerationStructureGeometry[] Geometries = geometries;
}
