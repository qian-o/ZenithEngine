using Graphics.Engine.Exceptions;

namespace Graphics.Engine.Vulkan.Helpers;

internal static class VulkanExtensions
{
    public static void ThrowCode(this VkResult result, string message = "")
    {
        if (result != VkResult.Success)
        {
            throw new BackendException($"Vulkan error: {result} {message}");
        }
    }
}
