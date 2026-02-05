using SharpMetal.Metal;
using SharpMetal.Foundation;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLFence : GraphicsResource
{
    private MTLSharedEvent sharedEvent;
    private ulong fenceValue;

    public MTLFence(GraphicsContext context) : base(context)
    {
        sharedEvent = Context.Device.NewSharedEvent();
        fenceValue = 0;
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public void Signal(MTLCommandBuffer commandBuffer)
    {
        fenceValue++;
        commandBuffer.EncodeSignalEvent(sharedEvent, fenceValue);
    }

    public void Wait(MTLCommandBuffer commandBuffer)
    {
        commandBuffer.EncodeWait(sharedEvent, fenceValue);
    }

    protected override void SetName(string name)
    {
        sharedEvent.Label = new NSString(name);
    }

    protected override void Destroy()
    {
        sharedEvent.Dispose();
    }
}
