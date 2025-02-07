using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct HitGroupDesc
{
    public HitGroupType Type;

    public string? ClosestHit;

    public string? AnyHit;

    public string? Intersection;

    public static HitGroupDesc New(HitGroupType type = HitGroupType.Triangles,
                                   string? closestHit = null,
                                   string? anyHit = null,
                                   string? intersection = null)
    {
        return new()
        {
            Type = type,
            ClosestHit = closestHit,
            AnyHit = anyHit,
            Intersection = intersection
        };
    }
}
