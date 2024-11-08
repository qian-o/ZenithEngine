using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;

namespace Graphics.Engine.Vulkan.Helpers;

internal static class VKHelpers
{
    public static uint GetBinding(LayoutElementDesc element)
    {
        return element.Type switch
        {
            ResourceType.ConstantBuffer => element.Slot,

            ResourceType.StructuredBufferReadWrite or
            ResourceType.TextureReadWrite => element.Slot + 20,

            ResourceType.Sampler => element.Slot + 40,

            ResourceType.StructuredBuffer or
            ResourceType.Texture or
            ResourceType.AccelerationStructure => element.Slot + 60,

            _ => 0
        };
    }
}
