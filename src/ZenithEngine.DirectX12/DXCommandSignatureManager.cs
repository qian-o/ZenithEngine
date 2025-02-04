using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXCommandSignatureManager(GraphicsContext context) : GraphicsResource(context)
{
    private readonly Lock drawSignaturesLock = new();
    private readonly Lock drawIndexedSignaturesLock = new();
    private readonly Dictionary<uint, ComPtr<ID3D12CommandSignature>> drawSignatures = [];
    private readonly Dictionary<uint, ComPtr<ID3D12CommandSignature>> drawIndexedSignatures = [];

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public ComPtr<ID3D12CommandSignature> GetDrawSignature(uint stride)
    {
        using Lock.Scope _ = drawSignaturesLock.EnterScope();

        if (!drawSignatures.TryGetValue(stride, out ComPtr<ID3D12CommandSignature> signature))
        {
            drawSignatures.Add(stride, signature = CreateSignature(IndirectArgumentType.Draw, stride));
        }

        return signature;
    }

    public ComPtr<ID3D12CommandSignature> GetDrawIndexedSignature(uint stride)
    {
        using Lock.Scope _ = drawIndexedSignaturesLock.EnterScope();

        if (!drawIndexedSignatures.TryGetValue(stride, out ComPtr<ID3D12CommandSignature> signature))
        {
            drawIndexedSignatures.Add(stride, signature = CreateSignature(IndirectArgumentType.DrawIndexed, stride));
        }

        return signature;
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        foreach (ComPtr<ID3D12CommandSignature> signature in drawSignatures.Values)
        {
            signature.Dispose();
        }

        foreach (ComPtr<ID3D12CommandSignature> signature in drawIndexedSignatures.Values)
        {
            signature.Dispose();
        }

        drawSignatures.Clear();
        drawIndexedSignatures.Clear();
    }

    private ComPtr<ID3D12CommandSignature> CreateSignature(IndirectArgumentType type, uint stride)
    {
        IndirectArgumentDesc argDesc = new()
        {
            Type = type
        };

        CommandSignatureDesc desc = new()
        {
            ByteStride = stride,
            NumArgumentDescs = 1,
            PArgumentDescs = &argDesc
        };

        Context.Device.CreateCommandSignature(&desc,
                                              (ComPtr<ID3D12RootSignature>)null,
                                              out ComPtr<ID3D12CommandSignature> signature).ThrowIfError();

        return signature;
    }
}
