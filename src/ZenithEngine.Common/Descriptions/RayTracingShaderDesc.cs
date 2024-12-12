using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct RayTracingShaderDesc
{
    public Shader RayGen;

    public Shader[] Miss;

    public HitGroupDesc[] HitGroups;

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
