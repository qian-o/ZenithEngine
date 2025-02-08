using Common;
using Hexa.NET.ImGui;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ShaderCompiler;

namespace ComputeShader;

internal class ComputeShaderTest(Backend backend) : VisualTest("Compute Shader Test", backend)
{
    private Texture output = null!;
    private ResourceLayout resourceLayout = null!;
    private ResourceSet resourceSet = null!;
    private ComputePipeline computePipeline = null!;

    protected override void OnLoad()
    {
        string hlsl = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", "Shader.hlsl"));

        TextureDesc outputDesc = new(Width, Height, usage: TextureUsage.ShaderResource | TextureUsage.UnorderedAccess);

        output = Context.Factory.CreateTexture(in outputDesc);

        ResourceLayoutDesc rlDesc = new([new(ShaderStages.Compute, ResourceType.TextureReadWrite, 0)]);

        resourceLayout = Context.Factory.CreateResourceLayout(in rlDesc);

        ResourceSetDesc rsDesc = new(resourceLayout, output);

        resourceSet = Context.Factory.CreateResourceSet(in rsDesc);

        using Shader csShader = Context.Factory.CompileShader(ShaderStages.Compute, hlsl, "CSMain");

        ComputePipelineDesc cpDesc = new(csShader, resourceLayout);

        computePipeline = Context.Factory.CreateComputePipeline(in cpDesc);
    }

    protected override void OnUpdate(double deltaTime, double totalTime)
    {
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

        ResourceSetDesc rsDesc = new(resourceLayout, output);

        resourceSet = Context.Factory.CreateResourceSet(in rsDesc);
    }

    protected override void OnDestroy()
    {
        computePipeline.Dispose();
        resourceSet.Dispose();
        resourceLayout.Dispose();
        output.Dispose();
    }
}
