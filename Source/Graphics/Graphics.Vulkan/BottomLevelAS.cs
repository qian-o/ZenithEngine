using Graphics.Vulkan.Descriptions;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class BottomLevelAS : VulkanObject<ulong>, IBindableResource
{
    internal BottomLevelAS(VulkanResources vkRes, ref readonly BottomLevelASDescription description) : base(vkRes, ObjectType.AccelerationStructureKhr)
    {
    }

    internal override ulong Handle { get; }

    internal override ulong[] GetHandles()
    {
        return [];
    }

    protected override void Destroy()
    {
    }
}
