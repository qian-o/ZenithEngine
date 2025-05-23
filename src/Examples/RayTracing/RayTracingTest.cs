using Common;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ShaderCompiler;

namespace RayTracing;

internal unsafe class RayTracingTest() : VisualTest("RayTracing Test")
{
    protected override void OnLoad()
    {
        string rayGeneration = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", "RayGen.slang");
        string miss = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", "Miss.slang");
        string closestHit = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", "ClosestHit.slang");

        using Shader _1 = Context.Factory.CompileShader(rayGeneration, ShaderStages.RayGeneration, "Main", out ShaderReflection rayGenerationReflection);
        using Shader _2 = Context.Factory.CompileShader(miss, ShaderStages.Miss, "Main", out ShaderReflection missReflection);
        using Shader _3 = Context.Factory.CompileShader(closestHit, ShaderStages.ClosestHit, "Main", out ShaderReflection closestHitReflection);
        ShaderReflection reflection = ShaderReflection.Merge(rayGenerationReflection, missReflection, closestHitReflection);
    }

    protected override void OnUpdate(double deltaTime, double totalTime)
    {
    }

    protected override void OnRender(double deltaTime, double totalTime)
    {
    }

    protected override void OnSizeChanged(uint width, uint height)
    {
    }

    protected override void OnDestroy()
    {
    }
}
