using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe abstract class VulkanObject<THandle>(VulkanResources vkRes, params ObjectType[] objectTypes) : DisposableObject
{
    private string name = string.Empty;

    internal abstract THandle Handle { get; }

    internal VulkanResources VkRes { get; } = vkRes;

    public string Name { get => name; set { name = value; UpdateResourceName(); } }

    internal abstract ulong[] GetHandles();

    private void UpdateResourceName()
    {
        if (!VkRes.IsInitializedGraphicsDevice)
        {
            return;
        }

        ulong[] handles = GetHandles();

        int length = Math.Min(handles.Length, objectTypes.Length);

        for (int i = 0; i < length; i++)
        {
            DebugUtilsObjectNameInfoEXT nameInfo = new()
            {
                SType = StructureType.DebugUtilsObjectNameInfoExt,
                ObjectType = objectTypes[i],
                ObjectHandle = handles[i],
                PObjectName = VkRes.Alloter.Allocate($"{Name} ({objectTypes[i]})")
            };

            VkRes.ExtDebugUtils?.SetDebugUtilsObjectName(VkRes.GetDevice(), &nameInfo).ThrowCode();
        }

        VkRes.Alloter.Clear();
    }
}
