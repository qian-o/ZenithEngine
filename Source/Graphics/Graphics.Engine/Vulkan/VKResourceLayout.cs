﻿using Graphics.Engine.Descriptions;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKResourceLayout : ResourceLayout
{
    public VKResourceLayout(Context context,
                            ref readonly ResourceLayoutDesc desc) : base(context, in desc)
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
