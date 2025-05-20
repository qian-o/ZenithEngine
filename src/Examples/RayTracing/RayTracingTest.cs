using Common;
using ZenithEngine.Common.Enums;
using ZenithEngine.ShaderCompiler;

namespace RayTracing;

internal unsafe class RayTracingTest() : VisualTest("RayTracing Test")
{
    protected override void OnLoad()
    {
        string ch = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", "ClosestHit.slang");

        Context.Factory.CompileShader(ch, ShaderStages.ClosestHit, "Main", out ShaderReflection chReflection);
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
