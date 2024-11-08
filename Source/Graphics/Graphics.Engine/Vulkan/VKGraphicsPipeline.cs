using Graphics.Engine.Descriptions;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKGraphicsPipeline : GraphicsPipeline
{
    public VKGraphicsPipeline(Context context,
                              ref readonly GraphicsPipelineDesc desc) : base(context, in desc)
    {
    }

    public new VKContext Context => (VKContext)base.Context;

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
