using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace ZenithEngine.DirectX12;

internal unsafe struct PipelineStateStream
{
    public PipelineStateStream(GraphicsPipelineStateDesc desc)
    {
        Flags = new(desc.Flags);
        NodeMask = new(desc.NodeMask);
        RootSignature = new(desc.PRootSignature);
        InputLayout = new SubInputLayout(desc.InputLayout);
        IBStripCutValue = new(desc.IBStripCutValue);
        PrimitiveTopology = new(desc.PrimitiveTopologyType);
        VS = new(desc.VS);
        GS = new(desc.GS);
        StreamOutput = new(desc.StreamOutput);
        HS = new(desc.HS);
        DS = new(desc.DS);
        PS = new(desc.PS);
        CS = new();
        Blend = new(desc.BlendState);
        DepthStencil1 = new(desc.DepthStencilState);
        DepthStencilFormat = new(desc.DSVFormat);
        Rasterizer = new(desc.RasterizerState);
        RenderTargetFormats = new(desc.NumRenderTargets, desc.RTVFormats);
        SampleDesc = new(desc.SampleDesc);
        SampleMask = new(desc.SampleMask);
        CachedPso = new(desc.CachedPSO);
    }

    public PipelineStateStream(ComputePipelineStateDesc desc)
    {
        Flags = new(desc.Flags);
        NodeMask = new(desc.NodeMask);
        RootSignature = new(desc.PRootSignature);
        InputLayout = new();
        IBStripCutValue = new();
        PrimitiveTopology = new();
        VS = new();
        GS = new();
        StreamOutput = new();
        HS = new();
        DS = new();
        PS = new();
        CS = new(desc.CS);
        Blend = new();
        DepthStencil1 = new();
        DepthStencilFormat = new();
        Rasterizer = new();
        RenderTargetFormats = new();
        SampleDesc = new();
        SampleMask = new();
        CachedPso = new(desc.CachedPSO);
    }

    public SubFlags Flags;

    public SubNodeMask NodeMask;

    public SubRootSignature RootSignature;

    public SubInputLayout InputLayout;

    public SubIBStripCutValue IBStripCutValue;

    public SubPrimitiveTopology PrimitiveTopology;

    public SubVS VS;

    public SubGS GS;

    public SubStreamOutput StreamOutput;

    public SubHS HS;

    public SubDS DS;

    public SubPS PS;

    public SubCS CS;

    public SubBlend Blend;

    public SubDepthStencil1 DepthStencil1;

    public SubDepthStencilFormat DepthStencilFormat;

    public SubRasterizer Rasterizer;

    public SubRenderTargetFormats RenderTargetFormats;

    public SubSampleDesc SampleDesc;

    public SubSampleMask SampleMask;

    public SubCachedPso CachedPso;
}

internal unsafe struct PipelineStateStream1
{
    public PipelineStateStream1(GraphicsPipelineStateDesc desc)
    {
        Flags = new(desc.Flags);
        NodeMask = new(desc.NodeMask);
        RootSignature = new(desc.PRootSignature);
        InputLayout = new SubInputLayout(desc.InputLayout);
        IBStripCutValue = new(desc.IBStripCutValue);
        PrimitiveTopology = new(desc.PrimitiveTopologyType);
        VS = new(desc.VS);
        GS = new(desc.GS);
        StreamOutput = new(desc.StreamOutput);
        HS = new(desc.HS);
        DS = new(desc.DS);
        PS = new(desc.PS);
        CS = new();
        Blend = new(desc.BlendState);
        DepthStencil1 = new(desc.DepthStencilState);
        DepthStencilFormat = new(desc.DSVFormat);
        Rasterizer = new(desc.RasterizerState);
        RenderTargetFormats = new(desc.NumRenderTargets, desc.RTVFormats);
        SampleDesc = new(desc.SampleDesc);
        SampleMask = new(desc.SampleMask);
        CachedPso = new(desc.CachedPSO);
        ViewInstancing = new();
    }

    public PipelineStateStream1(ComputePipelineStateDesc desc)
    {
        Flags = new(desc.Flags);
        NodeMask = new(desc.NodeMask);
        RootSignature = new(desc.PRootSignature);
        InputLayout = new();
        IBStripCutValue = new();
        PrimitiveTopology = new();
        VS = new();
        GS = new();
        StreamOutput = new();
        HS = new();
        DS = new();
        PS = new();
        CS = new(desc.CS);
        Blend = new();
        DepthStencil1 = new();
        DepthStencilFormat = new();
        Rasterizer = new();
        RenderTargetFormats = new();
        SampleDesc = new();
        SampleMask = new();
        CachedPso = new(desc.CachedPSO);
        ViewInstancing = new();
    }

    public SubFlags Flags;

    public SubNodeMask NodeMask;

    public SubRootSignature RootSignature;

    public SubInputLayout InputLayout;

    public SubIBStripCutValue IBStripCutValue;

    public SubPrimitiveTopology PrimitiveTopology;

    public SubVS VS;

    public SubGS GS;

    public SubStreamOutput StreamOutput;

    public SubHS HS;

    public SubDS DS;

    public SubPS PS;

    public SubCS CS;

    public SubBlend Blend;

    public SubDepthStencil1 DepthStencil1;

    public SubDepthStencilFormat DepthStencilFormat;

    public SubRasterizer Rasterizer;

    public SubRenderTargetFormats RenderTargetFormats;

    public SubSampleDesc SampleDesc;

    public SubSampleMask SampleMask;

    public SubCachedPso CachedPso;

    public SubViewInstancing ViewInstancing;
}

internal unsafe struct PipelineStateStream2
{
    public PipelineStateStream2(GraphicsPipelineStateDesc desc)
    {
        Flags = new(desc.Flags);
        NodeMask = new(desc.NodeMask);
        RootSignature = new(desc.PRootSignature);
        InputLayout = new SubInputLayout(desc.InputLayout);
        IBStripCutValue = new(desc.IBStripCutValue);
        PrimitiveTopology = new(desc.PrimitiveTopologyType);
        VS = new(desc.VS);
        GS = new(desc.GS);
        StreamOutput = new(desc.StreamOutput);
        HS = new(desc.HS);
        DS = new(desc.DS);
        PS = new(desc.PS);
        AS = new();
        MS = new();
        CS = new();
        Blend = new(desc.BlendState);
        DepthStencil1 = new(desc.DepthStencilState);
        DepthStencilFormat = new(desc.DSVFormat);
        Rasterizer = new(desc.RasterizerState);
        RenderTargetFormats = new(desc.NumRenderTargets, desc.RTVFormats);
        SampleDesc = new(desc.SampleDesc);
        SampleMask = new(desc.SampleMask);
        CachedPso = new(desc.CachedPSO);
        ViewInstancing = new();
    }

    public PipelineStateStream2(ComputePipelineStateDesc desc)
    {
        Flags = new(desc.Flags);
        NodeMask = new(desc.NodeMask);
        RootSignature = new(desc.PRootSignature);
        InputLayout = new();
        IBStripCutValue = new();
        PrimitiveTopology = new();
        VS = new();
        GS = new();
        StreamOutput = new();
        HS = new();
        DS = new();
        PS = new();
        AS = new();
        MS = new();
        CS = new(desc.CS);
        Blend = new();
        DepthStencil1 = new();
        DepthStencilFormat = new();
        Rasterizer = new();
        RenderTargetFormats = new();
        SampleDesc = new();
        SampleMask = new();
        CachedPso = new(desc.CachedPSO);
        ViewInstancing = new();
    }

    public SubFlags Flags;

    public SubNodeMask NodeMask;

    public SubRootSignature RootSignature;

    public SubInputLayout InputLayout;

    public SubIBStripCutValue IBStripCutValue;

    public SubPrimitiveTopology PrimitiveTopology;

    public SubVS VS;

    public SubGS GS;

    public SubStreamOutput StreamOutput;

    public SubHS HS;

    public SubDS DS;

    public SubPS PS;

    public SubAS AS;

    public SubMS MS;

    public SubCS CS;

    public SubBlend Blend;

    public SubDepthStencil1 DepthStencil1;

    public SubDepthStencilFormat DepthStencilFormat;

    public SubRasterizer Rasterizer;

    public SubRenderTargetFormats RenderTargetFormats;

    public SubSampleDesc SampleDesc;

    public SubSampleMask SampleMask;

    public SubCachedPso CachedPso;

    public SubViewInstancing ViewInstancing;
}

internal struct SubFlags(PipelineStateFlags flags)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.Flags;

    public PipelineStateFlags Flags = flags;
}

internal struct SubNodeMask(uint nodeMask)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.NodeMask;

    public uint NodeMask = nodeMask;
}

internal unsafe struct SubRootSignature(ID3D12RootSignature* rootSignature)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.RootSignature;

    public ID3D12RootSignature* RootSignature = rootSignature;
}

internal struct SubInputLayout(DxInputLayoutDesc inputLayout)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.InputLayout;

    public DxInputLayoutDesc InputLayout = inputLayout;
}

internal struct SubIBStripCutValue(IndexBufferStripCutValue ibStripCutValue)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.IBStripCutValue;

    public IndexBufferStripCutValue IBStripCutValue = ibStripCutValue;
}

internal struct SubPrimitiveTopology(PrimitiveTopologyType primitiveTopology)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.PrimitiveTopology;

    public PrimitiveTopologyType PrimitiveTopology = primitiveTopology;
}

internal struct SubVS(ShaderBytecode vs)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.VS;

    public ShaderBytecode VS = vs;
}

internal struct SubGS(ShaderBytecode gs)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.GS;

    public ShaderBytecode GS = gs;
}

internal struct SubStreamOutput(StreamOutputDesc streamOutput)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.StreamOutput;

    public StreamOutputDesc StreamOutput = streamOutput;
}

internal struct SubHS(ShaderBytecode hs)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.HS;

    public ShaderBytecode HS = hs;
}

internal struct SubDS(ShaderBytecode ds)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.DS;

    public ShaderBytecode DS = ds;
}

internal struct SubPS(ShaderBytecode ps)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.PS;

    public ShaderBytecode PS = ps;
}

internal struct SubAS(ShaderBytecode @as)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.As;

    public ShaderBytecode As = @as;
}

internal struct SubMS(ShaderBytecode ms)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.MS;

    public ShaderBytecode MS = ms;
}

internal struct SubCS(ShaderBytecode cs)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.CS;

    public ShaderBytecode CS = cs;
}

internal struct SubBlend(BlendDesc blend)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.Blend;

    public BlendDesc Blend = blend;
}

internal struct SubDepthStencil1(DepthStencilDesc1 depthStencil1)
{
    public SubDepthStencil1(DepthStencilDesc depthStencil) : this(new DepthStencilDesc1()
    {
        DepthEnable = depthStencil.DepthEnable,
        DepthWriteMask = depthStencil.DepthWriteMask,
        DepthFunc = depthStencil.DepthFunc,
        StencilEnable = depthStencil.StencilEnable,
        StencilReadMask = depthStencil.StencilReadMask,
        StencilWriteMask = depthStencil.StencilWriteMask,
        FrontFace = depthStencil.FrontFace,
        BackFace = depthStencil.BackFace,
        DepthBoundsTestEnable = 0
    })
    {
    }

    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.DepthStencil1;

    public DepthStencilDesc1 DepthStencil1 = depthStencil1;
}

internal struct SubDepthStencilFormat(Format depthStencilFormat)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.DepthStencilFormat;

    public Format DepthStencilFormat = depthStencilFormat;
}

internal struct SubRasterizer(RasterizerDesc rasterizer)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.Rasterizer;

    public RasterizerDesc Rasterizer = rasterizer;
}

internal struct SubRenderTargetFormats(RTFormatArray renderTargetFormats)
{
    public SubRenderTargetFormats(uint numRenderTargets, GraphicsPipelineStateDesc.RTVFormatsBuffer rTVFormats) : this(new RTFormatArray()
    {
        NumRenderTargets = numRenderTargets,
        RTFormats = new RTFormatArray.RTFormatsBuffer()
        {
            Element0 = rTVFormats.Element0,
            Element1 = rTVFormats.Element1,
            Element2 = rTVFormats.Element2,
            Element3 = rTVFormats.Element3,
            Element4 = rTVFormats.Element4,
            Element5 = rTVFormats.Element5,
            Element6 = rTVFormats.Element6,
            Element7 = rTVFormats.Element7
        }
    })
    {
    }

    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.RenderTargetFormats;

    public RTFormatArray RenderTargetFormats = renderTargetFormats;
}

internal struct SubSampleDesc(SampleDesc sampleDesc)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.SampleDesc;

    public SampleDesc SampleDesc = sampleDesc;
}

internal struct SubSampleMask(uint sampleMask)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.SampleMask;

    public uint SampleMask = sampleMask;
}

internal struct SubCachedPso(CachedPipelineState cachedPso)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.CachedPso;

    public CachedPipelineState CachedPso = cachedPso;
}

internal struct SubViewInstancing(ViewInstancingDesc viewInstancing)
{
    public readonly PipelineStateSubobjectType Type = PipelineStateSubobjectType.ViewInstancing;

    public ViewInstancingDesc ViewInstancing = viewInstancing;
}