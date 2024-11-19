using Silk.NET.Maths;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public class AccelerationStructureInstance(BottomLevelAS bottomLevel)
{
    public BottomLevelAS BottomLevel { get; } = bottomLevel;

    public uint InstanceID { get; set; }

    public uint InstanceContributionToHitGroupIndex { get; set; }

    public byte InstanceMask { get; set; }

    public Matrix4X4<float> Transform { get; set; }

    public AccelerationStructureInstanceOptions Options { get; set; }
}
