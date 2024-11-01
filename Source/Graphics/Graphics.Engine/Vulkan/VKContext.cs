using Silk.NET.Core;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal class VKContext : Context
{
    public static readonly Version32 Version = Vk.Version13;

    public VKContext()
    {
        Vk = Vk.GetApi();
    }

    public Vk Vk { get; }

    public VkInstance Instance { get; }

    protected override void Destroy()
    {
    }
}
