namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKCommandBuffer(Context context) : CommandBuffer(context)
{
    public new VKContext Context => (VKContext)base.Context;

    public override void Begin()
    {
    }

    protected override void EndInternal()
    {
    }

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
