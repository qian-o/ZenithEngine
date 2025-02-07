using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct TopLevelASDesc
{
    public AccelerationStructureInstance[] Instances;

    public uint OffsetInBytes;

    public AccelerationStructureBuildOptions Options;

    public static TopLevelASDesc New(AccelerationStructureInstance[] instances,
                                     uint offsetInBytes = 0,
                                     AccelerationStructureBuildOptions options = AccelerationStructureBuildOptions.PreferFastBuild)
    {
        return new()
        {
            Instances = instances,
            OffsetInBytes = offsetInBytes,
            Options = options
        };
    }
}
