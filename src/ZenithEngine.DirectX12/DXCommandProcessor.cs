using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXCommandProcessor : CommandProcessor
{
    public ComPtr<ID3D12CommandQueue> Queue;

    private readonly DXFence fence;

    public DXCommandProcessor(GraphicsContext context,
                              CommandProcessorType type) : base(context, type)
    {
        CommandQueueDesc desc = new()
        {
            Type = DXFormats.GetCommandListType(type),
            Priority = 0,
            Flags = CommandQueueFlags.None,
            NodeMask = 0
        };

        Context.Device.CreateCommandQueue(&desc, out Queue).ThrowIfError();

        fence = new(Context);
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public override void WaitIdle()
    {
        fence.Wait(this);
    }

    protected override CommandBuffer CreateCommandBuffer()
    {
        return new DXCommandBuffer(Context, this);
    }

    protected override void SubmitCommandBuffer(CommandBuffer commandBuffer)
    {
        Queue.ExecuteCommandLists(1, commandBuffer.DX().CommandList.GetAddressOf());
    }

    protected override void DebugName(string name)
    {
        Queue.SetName(name).ThrowIfError();
    }

    protected override void Destroy()
    {
        base.Destroy();

        fence.Dispose();

        Queue.Dispose();
    }
}
