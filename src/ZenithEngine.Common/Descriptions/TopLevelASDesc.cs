using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct TopLevelASDesc(AccelerationStructureInstance[] instances,
                             uint offsetInBytes = 0,
                             AccelerationStructureBuildOptions options = AccelerationStructureBuildOptions.PreferFastBuild)
{
    public AccelerationStructureInstance[] Instances = instances;

    public uint OffsetInBytes = offsetInBytes;

    public AccelerationStructureBuildOptions Options = options;
}
