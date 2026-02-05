using Metal;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLComputePipeline : ComputePipeline
{
    public MTLComputePipeline(GraphicsContext context,
                              ref readonly ComputePipelineDesc desc) : base(context, in desc)
    {
        if (desc.Shader is null)
        {
            throw new ArgumentNullException(nameof(desc.Shader), "Compute shader cannot be null");
        }

        MTLShader shader = (MTLShader)desc.Shader;

        using MTLComputePipelineDescriptor pipelineDesc = new();
        pipelineDesc.ComputeFunction = shader.Function;

        // Set thread group size if available from shader reflection
        // Metal will use the kernel's own threadGroupSize attribute if not specified

        PipelineState = Context.Device.CreateComputePipelineState(pipelineDesc, 0, null, out NSError? error)!;
        if (PipelineState is null || error is not null)
        {
            throw new InvalidOperationException($"Failed to create Metal compute pipeline state: {error?.LocalizedDescription}");
        }
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public IMTLComputePipelineState PipelineState { get; }

    protected override void SetName(string name)
    {
        PipelineState.Label = name;
    }

    protected override void Destroy()
    {
        PipelineState.Dispose();
    }
}
