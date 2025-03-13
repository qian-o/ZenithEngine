using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXCommandProcessor : CommandProcessor
{
    private readonly ComPtr<ID3D12CommandQueue> queue;
    private readonly DXFence fence;

    public DXCommandProcessor(GraphicsContext context,
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

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public override void WaitIdle()
    {
        fence.Wait(queue);
    }

    protected override CommandBuffer CreateCommandBuffer()
    {
        return new DXCommandBuffer(Context, this);
    }

    protected override void SubmitCommandBuffers(CommandBuffer[] commandBuffers)
    {
        ComPtr<ID3D12CommandList>[] commandLists = [.. commandBuffers.Select(static item => item.DX().CommandList)];

        fixed (ID3D12CommandList** pCommandLists = commandLists[0])
        {
            queue.ExecuteCommandLists((uint)commandBuffers.Length, pCommandLists);
        }
    }

    protected override void DebugName(string name)
    {
        queue.SetName(name).ThrowIfError();
    }

    protected override void Destroy()
    {
        base.Destroy();

        fence.Dispose();
    }
}
