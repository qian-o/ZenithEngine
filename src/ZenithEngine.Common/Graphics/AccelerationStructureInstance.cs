using Silk.NET.Maths;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public class AccelerationStructureInstance(BottomLevelAS bottomLevel)
{
    public BottomLevelAS BottomLevel { get; } = bottomLevel;

    public Matrix3X4<float> Transform { get; set; }

    public uint InstanceID { get; set; }

    public byte InstanceMask { get; set; }

    public uint InstanceContributionToHitGroupIndex { get; set; }

    public AccelerationStructureInstanceOptions Options { get; set; }
}
