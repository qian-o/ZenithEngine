using SharpMetal.Foundation;
using System;
using SharpMetal.Metal;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLMeshShaderPipeline : MeshShaderPipeline
{
    public MTLMeshShaderPipeline(GraphicsContext context,
                                 ref readonly MeshShaderPipelineDesc desc) : base(context, in desc)
    {
        // Metal supports mesh shaders through object and mesh shader stages
        // These are available on Apple7+ (A15, M2+) GPUs
        
        using MTLMeshRenderPipelineDescriptor pipelineDesc = new();

        // Set object shader (amplification shader in DX12 terminology)
        if (desc.Shaders.Amplification is not null)
        {
            MTLShader objectShader = (MTLShader)desc.Shaders.Amplification;
            pipelineDesc.ObjectFunction = objectShader.Function;
        }

        // Set mesh shader (required)
        if (desc.Shaders.Mesh is not null)
        {
            MTLShader meshShader = (MTLShader)desc.Shaders.Mesh;
            pipelineDesc.MeshFunction = meshShader.Function;
        }
        else
        {
            throw new ArgumentNullException(nameof(desc.Shaders.Mesh), "Mesh shader is required");
        }

        // Set fragment shader (pixel shader)
        if (desc.Shaders.Pixel is not null)
        {
            MTLShader fragmentShader = (MTLShader)desc.Shaders.Pixel;
            pipelineDesc.FragmentFunction = fragmentShader.Function;
        }

        // Configure color attachments
        for (int i = 0; i < desc.Outputs.ColorAttachments.Length; i++)
        {
            MTLRenderPipelineColorAttachmentDescriptor colorAttachment = pipelineDesc.GetColorAttachment((nuint)i);
            colorAttachment.PixelFormat = MTLFormats.GetMTLPixelFormat(desc.Outputs.ColorAttachments[i].Format);
            
            // Configure blending (same as graphics pipeline)
            var blendTarget = desc.RenderStates.BlendState.IndependentBlendEnabled
                ? (i < 8 ? GetBlendTargetDesc(desc.RenderStates.BlendState, i) : desc.RenderStates.BlendState.RenderTarget0)
                : desc.RenderStates.BlendState.RenderTarget0;

            colorAttachment.BlendingEnabled = blendTarget.BlendEnabled;
            if (blendTarget.BlendEnabled)
            {
                colorAttachment.SourceRgbBlendFactor = MTLFormats.GetMTLBlendFactor(blendTarget.SourceBlendColor);
                colorAttachment.DestinationRgbBlendFactor = MTLFormats.GetMTLBlendFactor(blendTarget.DestinationBlendColor);
                colorAttachment.RgbBlendOperation = MTLFormats.GetMTLBlendOperation(blendTarget.BlendOperationColor);
                colorAttachment.SourceAlphaBlendFactor = MTLFormats.GetMTLBlendFactor(blendTarget.SourceBlendAlpha);
                colorAttachment.DestinationAlphaBlendFactor = MTLFormats.GetMTLBlendFactor(blendTarget.DestinationBlendAlpha);
                colorAttachment.AlphaBlendOperation = MTLFormats.GetMTLBlendOperation(blendTarget.BlendOperationAlpha);
            }
            
            colorAttachment.WriteMask = MTLFormats.GetMTLColorWriteMask(blendTarget.ColorWriteChannels);
        }

        // Configure depth/stencil format
        if (desc.Outputs.DepthAttachment.HasValue)
        {
            pipelineDesc.DepthAttachmentPixelFormat = MTLFormats.GetMTLPixelFormat(desc.Outputs.DepthAttachment.Value.Format);
        }

        if (desc.Outputs.StencilAttachment.HasValue)
        {
            pipelineDesc.StencilAttachmentPixelFormat = MTLFormats.GetMTLPixelFormat(desc.Outputs.StencilAttachment.Value.Format);
        }

        // Configure multisampling
        pipelineDesc.SampleCount = MTLFormats.GetSampleCount(desc.Outputs.SampleCount);

        // Set rasterization rate (for VRS if supported)
        pipelineDesc.RasterSampleCount = MTLFormats.GetSampleCount(desc.Outputs.SampleCount);

        // Create the pipeline state
        PipelineState = Context.Device.CreateMeshRenderPipelineState(pipelineDesc, 0, null, null, out NSError? error)!;
        if (PipelineState is null || error is not null)
        {
            throw new InvalidOperationException($"Failed to create Metal mesh render pipeline state: {error?.LocalizedDescription}");
        }

        // Create depth/stencil state if needed (same as graphics pipeline)
        if (desc.RenderStates.DepthStencilState.DepthEnabled || desc.RenderStates.DepthStencilState.StencilEnabled)
        {
            using MTLDepthStencilDescriptor depthStencilDesc = new();
            depthStencilDesc.DepthCompareFunction = MTLFormats.GetMTLCompareFunction(desc.RenderStates.DepthStencilState.DepthFunction);
            depthStencilDesc.DepthWriteEnabled = desc.RenderStates.DepthStencilState.DepthWriteEnabled;

            if (desc.RenderStates.DepthStencilState.StencilEnabled)
            {
                // Configure stencil (same as graphics pipeline)
                using MTLStencilDescriptor frontFace = new();
                frontFace.StencilCompareFunction = MTLFormats.GetMTLCompareFunction(desc.RenderStates.DepthStencilState.FrontFace.StencilFunction);
                frontFace.StencilFailureOperation = MTLFormats.GetMTLStencilOperation(desc.RenderStates.DepthStencilState.FrontFace.StencilFailOperation);
                frontFace.DepthFailureOperation = MTLFormats.GetMTLStencilOperation(desc.RenderStates.DepthStencilState.FrontFace.StencilDepthFailOperation);
                frontFace.DepthStencilPassOperation = MTLFormats.GetMTLStencilOperation(desc.RenderStates.DepthStencilState.FrontFace.StencilPassOperation);
                frontFace.ReadMask = desc.RenderStates.DepthStencilState.StencilReadMask;
                frontFace.WriteMask = desc.RenderStates.DepthStencilState.StencilWriteMask;
                depthStencilDesc.FrontFaceStencil = frontFace;

                using MTLStencilDescriptor backFace = new();
                backFace.StencilCompareFunction = MTLFormats.GetMTLCompareFunction(desc.RenderStates.DepthStencilState.BackFace.StencilFunction);
                backFace.StencilFailureOperation = MTLFormats.GetMTLStencilOperation(desc.RenderStates.DepthStencilState.BackFace.StencilFailOperation);
                backFace.DepthFailureOperation = MTLFormats.GetMTLStencilOperation(desc.RenderStates.DepthStencilState.BackFace.StencilDepthFailOperation);
                backFace.DepthStencilPassOperation = MTLFormats.GetMTLStencilOperation(desc.RenderStates.DepthStencilState.BackFace.StencilPassOperation);
                backFace.ReadMask = desc.RenderStates.DepthStencilState.StencilReadMask;
                backFace.WriteMask = desc.RenderStates.DepthStencilState.StencilWriteMask;
                depthStencilDesc.BackFaceStencil = backFace;
            }

            DepthStencilState = Context.Device.NewDepthStencilState(depthStencilDesc);
        }

        // Store rasterizer state
        CullMode = desc.RenderStates.RasterizerState.CullMode;
        FrontFace = desc.RenderStates.RasterizerState.FrontFace;
        PrimitiveTopology = desc.PrimitiveTopology;
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public SharpMetal.Metal.MTLRenderPipelineState PipelineState { get; }

    public MTLDepthStencilState? DepthStencilState { get; }

    public ZenithEngine.Common.Enums.CullMode CullMode { get; }

    public ZenithEngine.Common.Enums.FrontFace FrontFace { get; }

    public ZenithEngine.Common.Enums.PrimitiveTopology PrimitiveTopology { get; }

    protected override void SetName(string name)
    {
        // PipelineState.Label is readonly in SharpMetal
        // Label must be set on the descriptor before pipeline creation
    }

    protected override void Destroy()
    {
        DepthStencilState?.Dispose();
        PipelineState.Dispose();
    }

    private static BlendStateRenderTargetDesc GetBlendTargetDesc(BlendStateDesc blendState, int index)
    {
        return index switch
        {
            0 => blendState.RenderTarget0,
            1 => blendState.RenderTarget1,
            2 => blendState.RenderTarget2,
            3 => blendState.RenderTarget3,
            4 => blendState.RenderTarget4,
            5 => blendState.RenderTarget5,
            6 => blendState.RenderTarget6,
            7 => blendState.RenderTarget7,
            _ => blendState.RenderTarget0
        };
    }
}
