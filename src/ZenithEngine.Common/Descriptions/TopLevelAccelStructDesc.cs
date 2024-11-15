using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct TopLevelASDesc
{
    public AccelerationStructureInstance[] Instances { get; set; }

    public uint Offset { get; set; }

    public AccelStructBuildOptions Options { get; set; }

    public static TopLevelASDesc Default(AccelerationStructureInstance[] instances,
                                         uint offset = 0,
                                         AccelStructBuildOptions options = AccelStructBuildOptions.PreferFastBuild)
    {
        return new()
        {
            Instances = instances,
            Offset = offset,
            Options = options
        };
    }
}
