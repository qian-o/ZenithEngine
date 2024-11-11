using Graphics.Engine.Enums;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKCommandProcessor : CommandProcessor
{
    public VKCommandProcessor(Context context, CommandProcessorType type) : base(context)
    {
        Queue = type switch
        {
            CommandProcessorType.Graphics => Context.GraphicsQueue,
            CommandProcessorType.Compute => Context.ComputeQueue,
            CommandProcessorType.Transfer => Context.TransferQueue,
            _ => throw new NotSupportedException()
        };
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkQueue Queue { get; }

    public override CommandBuffer CommandBuffer()
    {
        return new VKCommandBuffer(Context);
    }

    public override void Submit()
    {
    }

    public override void WaitIdle()
    {
    }

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
