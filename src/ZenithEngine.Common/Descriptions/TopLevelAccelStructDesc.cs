using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct TopLevelASDesc
{
    public AccelerationStructureInstance[] Instances;

    public uint Offset;

    public AccelerationStructureBuildOptions Options;

    public static TopLevelASDesc Default(AccelerationStructureInstance[] instances,
                                         uint offset = 0,
                                         AccelerationStructureBuildOptions options = AccelerationStructureBuildOptions.PreferFastBuild)
    {
        return new()
        {
            Instances = instances,
            Offset = offset,
            Options = options
        };
    }
}
