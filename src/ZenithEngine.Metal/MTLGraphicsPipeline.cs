using Metal;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLGraphicsPipeline : GraphicsPipeline
{
    public MTLGraphicsPipeline(GraphicsContext context,
                               ref readonly GraphicsPipelineDesc desc) : base(context, in desc)
    {
        using MTLRenderPipelineDescriptor pipelineDesc = new();

        // Set shaders
        if (desc.Shaders.VertexShader is not null)
        {
            MTLShader vertexShader = (MTLShader)desc.Shaders.VertexShader;
            pipelineDesc.VertexFunction = vertexShader.Function;
        }

        if (desc.Shaders.FragmentShader is not null)
        {
            MTLShader fragmentShader = (MTLShader)desc.Shaders.FragmentShader;
            pipelineDesc.FragmentFunction = fragmentShader.Function;
        }

        // Configure color attachments
        for (int i = 0; i < desc.Outputs.ColorAttachments.Length; i++)
        {
            MTLRenderPipelineColorAttachmentDescriptor colorAttachment = pipelineDesc.GetColorAttachment((nuint)i);
            colorAttachment.PixelFormat = MTLFormats.GetMTLPixelFormat(desc.Outputs.ColorAttachments[i].Format);
            
            // Configure blending
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

        // Configure vertex descriptor
        if (desc.InputLayouts.Length > 0)
        {
            using MTLVertexDescriptor vertexDescriptor = new();
            
            for (int i = 0; i < desc.InputLayouts.Length; i++)
            {
                InputLayoutDesc inputLayout = desc.InputLayouts[i];
                MTLVertexAttributeDescriptor attr = vertexDescriptor.GetAttribute((nuint)i);
                attr.Format = MTLFormats.GetMTLVertexFormat(inputLayout.Format);
                attr.Offset = inputLayout.Offset;
                attr.BufferIndex = inputLayout.Slot;

                MTLVertexBufferLayoutDescriptor bufferLayout = vertexDescriptor.GetLayout(inputLayout.Slot);
                bufferLayout.Stride = inputLayout.Stride;
                bufferLayout.StepFunction = MTLFormats.GetMTLVertexStepFunction(inputLayout.StepFunction);
                bufferLayout.StepRate = inputLayout.StepRate;
            }

            pipelineDesc.VertexDescriptor = vertexDescriptor;
        }

        // Create the pipeline state
        PipelineState = Context.Device.CreateRenderPipelineState(pipelineDesc, out NSError? error)!;
        if (PipelineState is null || error is not null)
        {
            throw new InvalidOperationException($"Failed to create Metal render pipeline state: {error?.LocalizedDescription}");
        }

        // Create depth/stencil state if needed
        if (desc.RenderStates.DepthStencilState.DepthEnabled || desc.RenderStates.DepthStencilState.StencilEnabled)
        {
            using MTLDepthStencilDescriptor depthStencilDesc = new();
            depthStencilDesc.DepthCompareFunction = MTLFormats.GetMTLCompareFunction(desc.RenderStates.DepthStencilState.DepthFunction);
            depthStencilDesc.DepthWriteEnabled = desc.RenderStates.DepthStencilState.DepthWriteEnabled;

            if (desc.RenderStates.DepthStencilState.StencilEnabled)
            {
                // Configure front face stencil
                using MTLStencilDescriptor frontFace = new();
                frontFace.StencilCompareFunction = MTLFormats.GetMTLCompareFunction(desc.RenderStates.DepthStencilState.FrontFace.StencilFunction);
                frontFace.StencilFailureOperation = MTLFormats.GetMTLStencilOperation(desc.RenderStates.DepthStencilState.FrontFace.StencilFailOperation);
                frontFace.DepthFailureOperation = MTLFormats.GetMTLStencilOperation(desc.RenderStates.DepthStencilState.FrontFace.StencilDepthFailOperation);
                frontFace.DepthStencilPassOperation = MTLFormats.GetMTLStencilOperation(desc.RenderStates.DepthStencilState.FrontFace.StencilPassOperation);
                frontFace.ReadMask = desc.RenderStates.DepthStencilState.StencilReadMask;
                frontFace.WriteMask = desc.RenderStates.DepthStencilState.StencilWriteMask;
                depthStencilDesc.FrontFaceStencil = frontFace;

                // Configure back face stencil
                using MTLStencilDescriptor backFace = new();
                backFace.StencilCompareFunction = MTLFormats.GetMTLCompareFunction(desc.RenderStates.DepthStencilState.BackFace.StencilFunction);
                backFace.StencilFailureOperation = MTLFormats.GetMTLStencilOperation(desc.RenderStates.DepthStencilState.BackFace.StencilFailOperation);
                backFace.DepthFailureOperation = MTLFormats.GetMTLStencilOperation(desc.RenderStates.DepthStencilState.BackFace.StencilDepthFailOperation);
                backFace.DepthStencilPassOperation = MTLFormats.GetMTLStencilOperation(desc.RenderStates.DepthStencilState.BackFace.StencilPassOperation);
                backFace.ReadMask = desc.RenderStates.DepthStencilState.StencilReadMask;
                backFace.WriteMask = desc.RenderStates.DepthStencilState.StencilWriteMask;
                depthStencilDesc.BackFaceStencil = backFace;
            }

            DepthStencilState = Context.Device.CreateDepthStencilState(depthStencilDesc);
        }

        // Store rasterizer state for command buffer usage
        CullMode = desc.RenderStates.RasterizerState.CullMode;
        FrontFace = desc.RenderStates.RasterizerState.FrontFace;
        PrimitiveTopology = desc.PrimitiveTopology;
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public IMTLRenderPipelineState PipelineState { get; }

    public IMTLDepthStencilState? DepthStencilState { get; }

    public ZenithEngine.Common.Enums.CullMode CullMode { get; }

    public ZenithEngine.Common.Enums.FrontFace FrontFace { get; }

    public ZenithEngine.Common.Enums.PrimitiveTopology PrimitiveTopology { get; }

    protected override void SetName(string name)
    {
        PipelineState.Label = name;
    }

    protected override void Destroy()
    {
        DepthStencilState?.Dispose();
        PipelineState.Dispose();
    }

    private static BlendStateRenderTargetDesc GetBlendTargetDesc(BlendState blendState, int index)
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
