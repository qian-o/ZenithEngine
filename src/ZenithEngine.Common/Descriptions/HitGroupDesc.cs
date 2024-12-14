using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct HitGroupDesc
{
    public Shader? ClosestHit;

    public Shader? AnyHit;

    public Shader? Intersection;

    public static HitGroupDesc Default(Shader? closestHit = null,
                                       Shader? anyHit = null,
                                       Shader? intersection = null)
    {
        return new()
        {
            ClosestHit = closestHit,
            AnyHit = anyHit,
            Intersection = intersection
        };
    }
}
