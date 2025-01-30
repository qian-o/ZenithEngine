using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXFence : GraphicsResource
{
    public ComPtr<ID3D12Fence> Fence;

    private readonly EventWaitHandle waitHandle;

    private ulong fenceValue;

    public DXFence(GraphicsContext context) : base(context)
    {
        Context.Device.CreateFence(0, FenceFlags.None, out Fence).ThrowIfError();

        waitHandle = new(false, EventResetMode.ManualReset);
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public void Wait(DXCommandProcessor processor)
    {
        fenceValue++;

        processor.Queue.Signal(Fence, fenceValue).ThrowIfError();

        if (Fence.GetCompletedValue() < fenceValue)
        {
            Fence.SetEventOnCompletion(fenceValue, (void*)waitHandle.SafeWaitHandle.DangerousGetHandle()).ThrowIfError();

            waitHandle.WaitOne();
            waitHandle.Reset();
        }
    }

    protected override void DebugName(string name)
    {
        Fence.SetName(name).ThrowIfError();
    }

    protected override void Destroy()
    {
        waitHandle.Dispose();

        Fence.Dispose();
    }
}
