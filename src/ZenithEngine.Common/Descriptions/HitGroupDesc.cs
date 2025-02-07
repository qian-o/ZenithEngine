using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct HitGroupDesc(HitGroupType type = HitGroupType.Triangles,
                           string? closestHit = null,
                           string? anyHit = null,
                           string? intersection = null)
{
    public HitGroupDesc()
    {
    }

    public HitGroupType Type = type;

    public string? ClosestHit = closestHit;

    public string? AnyHit = anyHit;

    public string? Intersection = intersection;
}
