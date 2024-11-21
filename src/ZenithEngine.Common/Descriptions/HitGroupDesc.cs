using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct HitGroupDesc
{
    public Shader? ClosestHit { get; set; }

    public Shader? AnyHit { get; set; }

    public Shader? Intersection { get; set; }

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
