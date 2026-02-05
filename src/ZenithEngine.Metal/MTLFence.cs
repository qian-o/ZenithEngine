using SharpMetal.Metal;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLFence : GraphicsResource
{
    private readonly MTLSharedEvent sharedEvent;
    private ulong fenceValue;

    public MTLFence(GraphicsContext context) : base(context)
    {
        sharedEvent = Context.Device.NewSharedEvent()!;
        fenceValue = 0;
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public void Wait(MTLCommandQueue queue)
    {
        fenceValue++;

        queue.EncodeSignalEvent(sharedEvent, fenceValue);

        // Wait for the GPU to complete work up to this fence value
        if (sharedEvent.SignaledValue < fenceValue)
        {
            using MTLSharedEventListener listener = new();
            using AutoResetEvent waitEvent = new(false);

            sharedEvent.NotifyListener(listener, fenceValue, (sharedEvent, value) =>
            {
                waitEvent.Set();
            });

            waitEvent.WaitOne();
        }
    }

    protected override void SetName(string name)
    {
        sharedEvent.Label = name;
    }

    protected override void Destroy()
    {
        sharedEvent.Dispose();
    }
}
