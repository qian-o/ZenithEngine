using System.Runtime.InteropServices;
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

internal struct SubObject<T>(PipelineStateSubobjectType type) where T : unmanaged
{
    public SubObject() : this(default)
    {
        Type = PipelineStateSubobjectType.MaxValid;
    }

    public readonly PipelineStateSubobjectType Type = type;

    public T Data = default;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubFlags
{
    public SubFlags()
    {
        Object = new(PipelineStateSubobjectType.Flags);
    }

    public SubFlags(PipelineStateFlags flags) : this()
    {
        Object.Data = flags;
    }

    [FieldOffset(0)]
    public SubObject<PipelineStateFlags> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubNodeMask
{
    public SubNodeMask()
    {
        Object = new(PipelineStateSubobjectType.NodeMask);
    }

    public SubNodeMask(uint nodeMask) : this()
    {
        Object.Data = nodeMask;
    }

    [FieldOffset(0)]
    public SubObject<uint> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct SubRootSignature
{
    public SubRootSignature()
    {
        Object = new(PipelineStateSubobjectType.RootSignature);
    }

    public SubRootSignature(ID3D12RootSignature* rootSignature) : this()
    {
        Object.Data = (nint)rootSignature;
    }

    [FieldOffset(0)]
    public SubObject<nint> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubInputLayout
{
    public SubInputLayout()
    {
        Object = new(PipelineStateSubobjectType.InputLayout);
    }

    public SubInputLayout(DxInputLayoutDesc inputLayout) : this()
    {
        Object.Data = inputLayout;
    }

    [FieldOffset(0)]
    public SubObject<DxInputLayoutDesc> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubIBStripCutValue
{
    public SubIBStripCutValue()
    {
        Object = new(PipelineStateSubobjectType.IBStripCutValue);
    }

    public SubIBStripCutValue(IndexBufferStripCutValue ibStripCutValue) : this()
    {
        Object.Data = ibStripCutValue;
    }

    [FieldOffset(0)]
    public SubObject<IndexBufferStripCutValue> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubPrimitiveTopology
{
    public SubPrimitiveTopology()
    {
        Object = new(PipelineStateSubobjectType.PrimitiveTopology);
    }

    public SubPrimitiveTopology(PrimitiveTopologyType primitiveTopology) : this()
    {
        Object.Data = primitiveTopology;
    }

    [FieldOffset(0)]
    public SubObject<PrimitiveTopologyType> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubVS
{
    public SubVS()
    {
        Object = new(PipelineStateSubobjectType.VS);
    }

    public SubVS(ShaderBytecode vs) : this()
    {
        Object.Data = vs;
    }

    [FieldOffset(0)]
    public SubObject<ShaderBytecode> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubGS
{
    public SubGS()
    {
        Object = new(PipelineStateSubobjectType.GS);
    }

    public SubGS(ShaderBytecode gs) : this()
    {
        Object.Data = gs;
    }

    [FieldOffset(0)]
    public SubObject<ShaderBytecode> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubStreamOutput
{
    public SubStreamOutput()
    {
        Object = new(PipelineStateSubobjectType.StreamOutput);
    }

    public SubStreamOutput(StreamOutputDesc streamOutput) : this()
    {
        Object.Data = streamOutput;
    }

    [FieldOffset(0)]
    public SubObject<StreamOutputDesc> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubHS
{
    public SubHS()
    {
        Object = new(PipelineStateSubobjectType.HS);
    }

    public SubHS(ShaderBytecode hs) : this()
    {
        Object.Data = hs;
    }

    [FieldOffset(0)]
    public SubObject<ShaderBytecode> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubDS
{
    public SubDS()
    {
        Object = new(PipelineStateSubobjectType.DS);
    }

    public SubDS(ShaderBytecode ds) : this()
    {
        Object.Data = ds;
    }

    [FieldOffset(0)]
    public SubObject<ShaderBytecode> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubPS
{
    public SubPS()
    {
        Object = new(PipelineStateSubobjectType.PS);
    }

    public SubPS(ShaderBytecode ps) : this()
    {
        Object.Data = ps;
    }

    [FieldOffset(0)]
    public SubObject<ShaderBytecode> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubAS
{
    public SubAS()
    {
        Object = new(PipelineStateSubobjectType.As);
    }

    public SubAS(ShaderBytecode as_) : this()
    {
        Object.Data = as_;
    }

    [FieldOffset(0)]
    public SubObject<ShaderBytecode> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubMS
{
    public SubMS()
    {
        Object = new(PipelineStateSubobjectType.MS);
    }

    public SubMS(ShaderBytecode ms) : this()
    {
        Object.Data = ms;
    }

    [FieldOffset(0)]
    public SubObject<ShaderBytecode> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubCS
{
    public SubCS()
    {
        Object = new(PipelineStateSubobjectType.CS);
    }

    public SubCS(ShaderBytecode cs) : this()
    {
        Object.Data = cs;
    }

    [FieldOffset(0)]
    public SubObject<ShaderBytecode> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubBlend
{
    public SubBlend()
    {
        Object = new(PipelineStateSubobjectType.Blend);
    }

    public SubBlend(BlendDesc blend) : this()
    {
        Object.Data = blend;
    }

    [FieldOffset(0)]
    public SubObject<BlendDesc> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubDepthStencil1
{
    public SubDepthStencil1()
    {
        Object = new(PipelineStateSubobjectType.DepthStencil1);
    }

    public SubDepthStencil1(DepthStencilDesc1 depthStencil) : this()
    {
        Object.Data = depthStencil;
    }

    public SubDepthStencil1(DepthStencilDesc depthStencilState) : this()
    {
        Object.Data = new()
        {
            DepthEnable = depthStencilState.DepthEnable,
            DepthWriteMask = depthStencilState.DepthWriteMask,
            DepthFunc = depthStencilState.DepthFunc,
            StencilEnable = depthStencilState.StencilEnable,
            StencilReadMask = depthStencilState.StencilReadMask,
            StencilWriteMask = depthStencilState.StencilWriteMask,
            FrontFace = depthStencilState.FrontFace,
            BackFace = depthStencilState.BackFace,
            DepthBoundsTestEnable = 0
        };
    }

    [FieldOffset(0)]
    public SubObject<DepthStencilDesc1> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubDepthStencilFormat
{
    public SubDepthStencilFormat()
    {
        Object = new(PipelineStateSubobjectType.DepthStencilFormat);
    }

    public SubDepthStencilFormat(Format depthStencilFormat) : this()
    {
        Object.Data = depthStencilFormat;
    }

    [FieldOffset(0)]
    public SubObject<Format> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubRasterizer
{
    public SubRasterizer()
    {
        Object = new(PipelineStateSubobjectType.Rasterizer);
    }

    public SubRasterizer(RasterizerDesc rasterizer) : this()
    {
        Object.Data = rasterizer;
    }

    [FieldOffset(0)]
    public SubObject<RasterizerDesc> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubRenderTargetFormats
{
    public SubRenderTargetFormats()
    {
        Object = new(PipelineStateSubobjectType.RenderTargetFormats);
    }

    public SubRenderTargetFormats(RTFormatArray renderTargetFormats) : this()
    {
        Object.Data = renderTargetFormats;
    }

    public SubRenderTargetFormats(uint numRenderTargets, GraphicsPipelineStateDesc.RTVFormatsBuffer rTVFormats) : this()
    {
        Object.Data = new()
        {
            NumRenderTargets = numRenderTargets,
            RTFormats = new()
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
        };
    }

    [FieldOffset(0)]
    public SubObject<RTFormatArray> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubSampleDesc
{
    public SubSampleDesc()
    {
        Object = new(PipelineStateSubobjectType.SampleDesc);
    }

    public SubSampleDesc(SampleDesc sampleDesc) : this()
    {
        Object.Data = sampleDesc;
    }

    [FieldOffset(0)]
    public SubObject<SampleDesc> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubSampleMask
{
    public SubSampleMask()
    {
        Object = new(PipelineStateSubobjectType.SampleMask);
    }

    public SubSampleMask(uint sampleMask) : this()
    {
        Object.Data = sampleMask;
    }

    [FieldOffset(0)]
    public SubObject<uint> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubCachedPso
{
    public SubCachedPso()
    {
        Object = new(PipelineStateSubobjectType.CachedPso);
    }

    public SubCachedPso(CachedPipelineState cachedPso) : this()
    {
        Object.Data = cachedPso;
    }

    [FieldOffset(0)]
    public SubObject<CachedPipelineState> Object;

    [FieldOffset(0)]
    internal nint padding;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SubViewInstancing
{
    public SubViewInstancing()
    {
        Object = new(PipelineStateSubobjectType.ViewInstancing);
    }

    public SubViewInstancing(ViewInstancingDesc viewInstancing) : this()
    {
        Object.Data = viewInstancing;
    }

    [FieldOffset(0)]
    public SubObject<ViewInstancingDesc> Object;

    [FieldOffset(0)]
    internal nint padding;
}