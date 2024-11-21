using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct RayTracingShaderDesc
{
    public Shader RayGen { get; set; }

    public Shader[] Miss { get; set; }

    public HitGroupDesc[] HitGroups { get; set; }

    public static RayTracingShaderDesc Default(Shader rayGen,
                                               Shader[] miss,
                                               HitGroupDesc[] hitGroups)
    {
        return new()
        {
            RayGen = rayGen,
            Miss = miss,
            HitGroups = hitGroups
        };
    }
}
