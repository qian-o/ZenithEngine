using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal class DXCommandProcessor : CommandProcessor
{
    public ComPtr<ID3D12CommandQueue> Queue;

    private readonly DXFence fence;

    public DXCommandProcessor(GraphicsContext context,
                              CommandProcessorType type) : base(context, type)
    {
        CommandQueueDesc desc = new()
        {
            Type = type switch
            {
                CommandProcessorType.Graphics => CommandListType.Direct,
                CommandProcessorType.Compute => CommandListType.Compute,
                CommandProcessorType.Copy => CommandListType.Copy,
                _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(type))
            },
            Priority = 0,
            Flags = CommandQueueFlags.None,
            NodeMask = 0
        };

        Context.Device.CreateCommandQueue(in desc, out Queue);

        fence = new(Context);
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public override void WaitIdle()
    {
        fence.Wait(this);
    }

    protected override CommandBuffer CreateCommandBuffer()
    {
        throw new NotImplementedException();
    }

    protected override void SubmitCommandBuffer(CommandBuffer commandBuffer)
    {
        throw new NotImplementedException();
    }

    protected override void DebugName(string name)
    {
        Queue.SetName(name);
    }

    protected override void Destroy()
    {
        base.Destroy();

        fence.Dispose();

        Queue.Dispose();
    }
}
