using SkiaSharp;

namespace Graphics.Vulkan;

public static class SkiaVk
{
    public static SKSurface CreateSurface(Texture texture)
    {
        GRVkBackendContext backendContext = new()
        {
            VkInstance = texture.VkRes.Instance.Handle,
            VkPhysicalDevice = texture.VkRes.VkPhysicalDevice.Handle,
            VkDevice = texture.VkRes.VkDevice.Handle,
            VkQueue = texture.VkRes.GraphicsDevice.GraphicsExecutor.Handle.Handle,
            GraphicsQueueIndex = texture.VkRes.GraphicsDevice.GraphicsExecutor.FamilyIndex,
            MaxAPIVersion = Context.ApiVersion,
            GetProcedureAddress = GetProcedureAddress,
        };

        GRContext context = GRContext.CreateVulkan(backendContext);

        GRVkImageInfo imageInfo = new();
        imageInfo.Image = texture.Handle.Handle;

        nint GetProcedureAddress(string name, nint instance, nint device)
        {
            if (instance != 0)
            {
                return texture.VkRes.Vk.GetInstanceProcAddr(texture.VkRes.Instance, name);
            }

            if (device != 0)
            {
                return texture.VkRes.Vk.GetDeviceProcAddr(texture.VkRes.VkDevice, name);
            }

            return 0;
        }
    }
}
