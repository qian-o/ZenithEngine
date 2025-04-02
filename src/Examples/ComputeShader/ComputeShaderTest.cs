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
    private ResourceLayout resourceLayout = null!;
    private ResourceSet resourceSet = null!;
    private ComputePipeline computePipeline = null!;

    protected override void OnLoad()
    {
        string hlsl = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", "Shader.hlsl"));

        BufferDesc cbDesc = new((uint)sizeof(Constants), BufferUsage.ConstantBuffer | BufferUsage.Dynamic);

        constantsBuffer = Context.Factory.CreateBuffer(in cbDesc);

        TextureDesc outputDesc = new(Width, Height, usage: TextureUsage.ShaderResource | TextureUsage.UnorderedAccess);

        output = Context.Factory.CreateTexture(in outputDesc);

        ResourceLayoutDesc rlDesc = new
        (
            new(ShaderStages.Compute, ResourceType.ConstantBuffer, 0),
            new(ShaderStages.Compute, ResourceType.TextureReadWrite, 0)
        );

        resourceLayout = Context.Factory.CreateResourceLayout(in rlDesc);

        ResourceSetDesc rsDesc = new(resourceLayout, constantsBuffer, output);

        resourceSet = Context.Factory.CreateResourceSet(in rsDesc);

        using Shader csShader = Context.Factory.CompileShader(ShaderStages.Compute, hlsl, "CSMain");

        ComputePipelineDesc cpDesc = new(csShader, resourceLayout);

        computePipeline = Context.Factory.CreateComputePipeline(in cpDesc);
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

        commandBuffer.PrepareResources([resourceSet]);

        commandBuffer.SetComputePipeline(computePipeline);
        commandBuffer.SetResourceSet(0, resourceSet);

        commandBuffer.Dispatch((uint)Math.Ceiling(Width / 8.0), (uint)Math.Ceiling(Height / 8.0), 1);

        commandBuffer.End();
        commandBuffer.Commit();
    }

    protected override void OnSizeChanged(uint width, uint height)
    {
        resourceSet.Dispose();
        output.Dispose();

        ImGuiController.RemoveBinding(output);

        TextureDesc outputDesc = new(width, height, usage: TextureUsage.ShaderResource | TextureUsage.UnorderedAccess);

        output = Context.Factory.CreateTexture(in outputDesc);

        ResourceSetDesc rsDesc = new(resourceLayout, constantsBuffer, output);

        resourceSet = Context.Factory.CreateResourceSet(in rsDesc);
    }

    protected override void OnDestroy()
    {
        computePipeline.Dispose();
        resourceSet.Dispose();
        resourceLayout.Dispose();
        output.Dispose();
        constantsBuffer.Dispose();
    }
}
