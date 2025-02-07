using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct RayTracingShaderDesc(Shader rayGen,
                                   Shader[] miss,
                                   Shader[] closestHit,
                                   Shader[]? anyHit = null,
                                   Shader[]? intersection = null)
{
    public RayTracingShaderDesc()
    {
    }

    public Shader RayGen = rayGen;

    public Shader[] Miss = miss;

    public Shader[] ClosestHit = closestHit;

    public Shader[] AnyHit = anyHit ?? [];

    public Shader[] Intersection = intersection ?? [];
}
