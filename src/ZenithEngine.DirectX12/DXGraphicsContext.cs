using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXGraphicsContext : GraphicsContext
{
    public const int DefaultShader4ComponentMapping = 0x1688;

    public ComPtr<IDXGIFactory6> Factory6;
    public ComPtr<IDXGIAdapter> Adapter;
    public ComPtr<ID3D12Device> Device;

    public ComPtr<ID3D12Device5> Device5;

    public ComPtr<ID3D12CommandQueue> GraphicsQueue;
    public ComPtr<ID3D12CommandQueue> ComputeQueue;
    public ComPtr<ID3D12CommandQueue> CopyQueue;

    public ComPtr<ID3D12CommandSignature> DrawSignature;
    public ComPtr<ID3D12CommandSignature> DrawIndexedSignature;
    public ComPtr<ID3D12CommandSignature> DispatchSignature;

    public DXGraphicsContext()
    {
        D3D12 = D3D12.GetApi();
        DXGI = DXGI.GetApi(null);
        Backend = Backend.DirectX12;
        Capabilities = new(this);
        Factory = new(this);
    }

    public D3D12 D3D12 { get; }

    public DXGI DXGI { get; }

    public DXDebug? Debug { get; private set; }

    public DXDescriptorAllocator? RtvAllocator { get; private set; }

    public DXDescriptorAllocator? DsvAllocator { get; private set; }

    public DXDescriptorAllocator? CbvSrvUavAllocator { get; private set; }

    public DXDescriptorAllocator? SamplerAllocator { get; private set; }

    public override Backend Backend { get; }

    public override DXDeviceCapabilities Capabilities { get; }

    public override DXResourceFactory Factory { get; }

    public override MappedResource MapMemory(Buffer buffer, MapMode mode)
    {
        void* data = null;
        buffer.DX().Resource.Map(0, (Range*)null, &data).ThrowIfError();

        return new(buffer, mode, (nint)data, buffer.Desc.SizeInBytes);
    }

    public override void UnmapMemory(Buffer buffer)
    {
        buffer.DX().Resource.Unmap(0, (Range*)null);
    }

    protected override void CreateDeviceInternal(bool useDebugLayer)
    {
        if (Factory6.Handle is not null)
        {
            return;
        }

        if (useDebugLayer)
        {
            D3D12.GetDebugInterface(out ComPtr<ID3D12Debug> debugInterface).ThrowIfError();

            debugInterface.EnableDebugLayer();

            debugInterface.Dispose();
        }

        DXGI.CreateDXGIFactory1(out Factory6).ThrowIfError();

        Factory6.EnumAdapterByGpuPreference(0, GpuPreference.HighPerformance, out Adapter).ThrowIfError();

        D3D12.CreateDevice(Adapter, D3DFeatureLevel.Level120, out Device).ThrowIfError();

        Device.QueryInterface(out Device5).ThrowIfError(true);

        CommandQueueDesc commandQueueDesc = new()
        {
            Type = CommandListType.Direct,
            Priority = 0,
            Flags = CommandQueueFlags.None,
            NodeMask = 0
        };

        Device.CreateCommandQueue(&commandQueueDesc, out GraphicsQueue).ThrowIfError();

        commandQueueDesc.Type = CommandListType.Compute;

        Device.CreateCommandQueue(&commandQueueDesc, out ComputeQueue).ThrowIfError();

        commandQueueDesc.Type = CommandListType.Copy;

        Device.CreateCommandQueue(&commandQueueDesc, out CopyQueue).ThrowIfError();

        IndirectArgumentDesc indirectArgumentDesc = new()
        {
            Type = IndirectArgumentType.Draw
        };

        CommandSignatureDesc commandSignatureDesc = new()
        {
            ByteStride = (uint)sizeof(IndirectDrawArgs),
            NumArgumentDescs = 1,
            PArgumentDescs = &indirectArgumentDesc
        };

        Device.CreateCommandSignature(&commandSignatureDesc,
                                      (ComPtr<ID3D12RootSignature>)null,
                                      out DrawSignature).ThrowIfError();

        indirectArgumentDesc.Type = IndirectArgumentType.DrawIndexed;
        commandSignatureDesc.ByteStride = (uint)sizeof(IndirectDrawIndexedArgs);

        Device.CreateCommandSignature(&commandSignatureDesc,
                                      (ComPtr<ID3D12RootSignature>)null,
                                      out DrawIndexedSignature).ThrowIfError();

        indirectArgumentDesc.Type = IndirectArgumentType.Dispatch;
        commandSignatureDesc.ByteStride = (uint)sizeof(IndirectDispatchArgs);

        Device.CreateCommandSignature(&commandSignatureDesc,
                                      (ComPtr<ID3D12RootSignature>)null,
                                      out DispatchSignature).ThrowIfError();

        Debug = useDebugLayer ? new(this) : null;
        RtvAllocator = new(this, DescriptorHeapType.Rtv, 128);
        DsvAllocator = new(this, DescriptorHeapType.Dsv, 128);
        CbvSrvUavAllocator = new(this, DescriptorHeapType.CbvSrvUav, 409600);
        SamplerAllocator = new(this, DescriptorHeapType.Sampler, 32);

        Capabilities.Init();
    }

    protected override void DestroyInternal()
    {
        SamplerAllocator?.Dispose();
        CbvSrvUavAllocator?.Dispose();
        DsvAllocator?.Dispose();
        RtvAllocator?.Dispose();
        Debug?.Dispose();

        DispatchSignature.Dispose();
        DrawIndexedSignature.Dispose();
        DrawSignature.Dispose();

        CopyQueue.Dispose();
        ComputeQueue.Dispose();
        GraphicsQueue.Dispose();

        Device5.Dispose();

        Device.Dispose();
        Adapter.Dispose();
        Factory6.Dispose();

        DXGI.Dispose();
        D3D12.Dispose();

        Debug = null;
        RtvAllocator = null;
        DsvAllocator = null;
        CbvSrvUavAllocator = null;
        SamplerAllocator = null;
    }
}
