using Graphics.Core;

namespace Graphics.Engine.Vulkan;

internal abstract class VKDeviceResource(VKContext context) : DisposableObject
{
    public VKContext Context { get; } = context;
}
