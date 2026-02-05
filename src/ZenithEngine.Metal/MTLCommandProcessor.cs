using Metal;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLCommandProcessor : CommandProcessor
{
    private readonly IMTLCommandQueue queue;
    private readonly MTLFence fence;

    public MTLCommandProcessor(GraphicsContext context,
                               CommandProcessorType type) : base(context, type)
    {
        queue = type switch
        {
            CommandProcessorType.Graphics => Context.GraphicsQueue,
            CommandProcessorType.Compute => Context.ComputeQueue,
            CommandProcessorType.Copy => Context.CopyQueue,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(type))
        };

        fence = new(Context);
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public override void WaitIdle()
    {
        fence.Wait(queue);
    }

    protected override CommandBuffer CreateCommandBuffer()
    {
        return new MTLCommandBuffer(Context, this);
    }

    protected override void SubmitCommandBuffers(CommandBuffer[] commandBuffers)
    {
        foreach (CommandBuffer cmd in commandBuffers)
        {
            MTLCommandBuffer mtlCmd = (MTLCommandBuffer)cmd;
            if (mtlCmd.CommandBuffer is not null)
            {
                mtlCmd.CommandBuffer.Commit();
            }
        }
    }

    protected override void SetName(string name)
    {
        queue.Label = name;
    }

    protected override void Destroy()
    {
        base.Destroy();

        fence.Dispose();
    }
}
