using SharpMetal.Foundation;
using System;
using SharpMetal.Metal;
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

        MTLComputePipelineDescriptor pipelineDesc = new();
        pipelineDesc.ComputeFunction = shader.Function;

        // Set thread group size if available from shader reflection
        // Metal will use the kernel's own threadGroupSize attribute if not specified

        NSError error = new(IntPtr.Zero);
        PipelineState = Context.Device.NewComputePipelineState(pipelineDesc, 0, IntPtr.Zero, ref error);
        if (error != IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to create Metal compute pipeline state: {error.LocalizedDescription}");
        }
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public SharpMetal.Metal.MTLComputePipelineState PipelineState { get; }

    protected override void SetName(string name)
    {
        // Label is readonly in SharpMetal - set during descriptor creation if needed
    }

    protected override void Destroy()
    {
        PipelineState.Dispose();
    }
}
