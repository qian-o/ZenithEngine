using Graphics.Core;
using Graphics.Core.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public abstract unsafe class VulkanObject<THandle>(VulkanResources vkRes, params ObjectType[] objectTypes) : DisposableObject
{
    private string name = string.Empty;

    internal abstract THandle Handle { get; }

    internal VulkanResources VkRes { get; } = vkRes;

    internal Alloter Alloter { get; } = new();

    public string Name { get => name; set { name = value; UpdateResourceName(); } }

    internal abstract ulong[] GetHandles();

    protected override void Destroy()
    {
        Alloter.Dispose();
    }

    private void UpdateResourceName()
    {
        if (!VkRes.IsInitializedGraphicsDevice)
        {
            return;
        }

        VkRes.VkDebug?.SetObjectName(this, objectTypes);
    }
}
