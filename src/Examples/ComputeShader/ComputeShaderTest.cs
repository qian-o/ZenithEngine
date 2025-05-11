using System.Runtime.InteropServices;
using Common;
using Hexa.NET.ImGui;
using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ShaderCompiler;
using Buffer = ZenithEngine.Common.Graphics.Buffer;

namespace ComputeShader;

internal unsafe class ComputeShaderTest() : VisualTest("Compute Shader Test")
{
    [StructLayout(LayoutKind.Explicit)]
    private struct Constants
    {
        [FieldOffset(0)]
        public Vector2D<float> Resolution;

        [FieldOffset(8)]
        public float TotalTime;
    }

    private Buffer constantsBuffer = null!;
    private Texture output = null!;
    private ResourceLayout layout = null!;
    private ResourceSet set = null!;
    private ComputePipeline pipeline = null!;

    protected override void OnLoad()
    {
        string shader = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", "Shader.slang");

        BufferDesc cbDesc = new((uint)sizeof(Constants), BufferUsage.ConstantBuffer | BufferUsage.Dynamic);

        constantsBuffer = Context.Factory.CreateBuffer(in cbDesc);

        TextureDesc outputDesc = new(Width, Height, usage: TextureUsage.ShaderResource | TextureUsage.UnorderedAccess);

        output = Context.Factory.CreateTexture(in outputDesc);

        ShaderReflection reflection = ShaderReflection.Empty;
        using Shader csShader = Context.Factory.CompileShader(shader, ShaderStages.Compute, "Main", ref reflection);

        ResourceLayoutDesc layoutDesc = new
        (
            reflection["constants"].Desc,
            reflection["output"].Desc
        );

        layout = Context.Factory.CreateResourceLayout(in layoutDesc);

        ResourceSetDesc rsDesc = new(layout, constantsBuffer, output);

        set = Context.Factory.CreateResourceSet(in rsDesc);

        ComputePipelineDesc cpDesc = new(csShader, layout);

        pipeline = Context.Factory.CreateComputePipeline(in cpDesc);
    }

    protected override void OnUpdate(double deltaTime, double totalTime)
    {
        Constants constants = new()
        {
            Resolution = new(Width, Height),
            TotalTime = (float)totalTime
        };

        Context.UpdateBuffer(constantsBuffer, (nint)(&constants), (uint)sizeof(Constants));

        ImGui.GetBackgroundDrawList().AddImage(ImGuiController.GetBinding(output), new(0, 0), new(Width, Height));
    }

    protected override void OnRender(double deltaTime, double totalTime)
    {
        CommandBuffer commandBuffer = CommandProcessor.CommandBuffer();

        commandBuffer.Begin();

        commandBuffer.PrepareResources([set]);

        commandBuffer.SetComputePipeline(pipeline);
        commandBuffer.SetResourceSet(0, set);

        commandBuffer.Dispatch((uint)Math.Ceiling(Width / 32.0), (uint)Math.Ceiling(Height / 32.0), 1);

        commandBuffer.End();
        commandBuffer.Commit();
    }

    protected override void OnSizeChanged(uint width, uint height)
    {
        set.Dispose();
        output.Dispose();

        ImGuiController.RemoveBinding(output);

        TextureDesc outputDesc = new(width, height, usage: TextureUsage.ShaderResource | TextureUsage.UnorderedAccess);

        output = Context.Factory.CreateTexture(in outputDesc);

        ResourceSetDesc rsDesc = new(layout, constantsBuffer, output);

        set = Context.Factory.CreateResourceSet(in rsDesc);
    }

    protected override void OnDestroy()
    {
        pipeline.Dispose();
        set.Dispose();
        layout.Dispose();
        output.Dispose();
        constantsBuffer.Dispose();
    }
}
