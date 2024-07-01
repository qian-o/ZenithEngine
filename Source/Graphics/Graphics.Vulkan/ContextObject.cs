using Graphics.Core;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Vulkan;

public abstract class ContextObject(Context context) : DisposableObject
{
    internal Context Context => context;

    internal Alloter Alloter => context.Alloter;

    internal Vk Vk => context.Vk;

    internal Instance Instance => context.Instance;

    internal KhrSurface SurfaceExt => context.SurfaceExt;
}
