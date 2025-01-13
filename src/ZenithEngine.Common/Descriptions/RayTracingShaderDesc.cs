using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct RayTracingShaderDesc
{
    public Shader RayGen;

    public Shader[] Miss;

    public Shader[] ClosestHit;

    public Shader[] AnyHit;

    public Shader[] Intersection;

    public static RayTracingShaderDesc Default(Shader rayGen,
                                               Shader[] miss,
                                               Shader[] closestHit,
                                               Shader[] anyHit,
                                               Shader[] intersection)
    {
        return new()
        {
            RayGen = rayGen,
            Miss = miss,
            ClosestHit = closestHit,
            AnyHit = anyHit,
            Intersection = intersection
        };
    }
}
