using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct HitGroupDesc(string name,
                           HitGroupType type = HitGroupType.Triangles,
                           string? closestHit = null,
                           string? anyHit = null,
                           string? intersection = null)
{
    public HitGroupDesc() : this(string.Empty, HitGroupType.Triangles, null, null, null)
    {
    }

    public string Name = name;

    public HitGroupType Type = type;

    public string? ClosestHit = closestHit;

    public string? AnyHit = anyHit;

    public string? Intersection = intersection;
}
