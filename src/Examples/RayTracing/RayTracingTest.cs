using Common;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ShaderCompiler;

namespace RayTracing;

internal unsafe class RayTracingTest() : VisualTest("RayTracing Test")
{
    private readonly string shaderPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders");

    protected override void OnLoad()
    {
        string rayGeneration = Path.Combine(shaderPath, "RayGen.slang");
        string miss = Path.Combine(shaderPath, "Miss.slang");
        string closestHit = Path.Combine(shaderPath, "ClosestHit.slang");

        using Shader _1 = Context.Factory.CompileShader(rayGeneration, ShaderStages.RayGeneration, "Main", out ShaderReflection r1);
        using Shader _2 = Context.Factory.CompileShader(miss, ShaderStages.Miss, "Main", out ShaderReflection r2);
        using Shader _3 = Context.Factory.CompileShader(miss, ShaderStages.Miss, "ShadowMain", out ShaderReflection r3);
        using Shader _4 = Context.Factory.CompileShader(closestHit, ShaderStages.ClosestHit, "Main", out ShaderReflection r4);
        ShaderReflection reflection = ShaderReflection.Merge(r1, r2, r3, r4);
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
