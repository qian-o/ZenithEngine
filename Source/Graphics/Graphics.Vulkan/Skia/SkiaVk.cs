using Silk.NET.Vulkan;
using SkiaSharp;

namespace Graphics.Vulkan;

public static class SkiaVk
{
    private static readonly Dictionary<VkDevice, GRContext> _cache = [];

    public static SKSurface CreateSurface(Texture texture)
    {
        GRVkImageInfo imageInfo = new()
        {
            Image = texture.Handle.Handle,
            Alloc = new GRVkAlloc() { Memory = texture.DeviceMemory!.Handle.Handle, Size = texture.DeviceMemory.SizeInBytes },
            ImageTiling = (uint)ImageTiling.Optimal,
            ImageLayout = (uint)texture.ImageLayouts[0],
            Format = (uint)Formats.GetPixelFormat(texture.Format, false),
            ImageUsageFlags = (uint)Formats.GetImageUsageFlags(texture.Usage),
            SampleCount = (uint)Formats.GetSampleCount(texture.SampleCount),
            LevelCount = texture.MipLevels,
            CurrentQueueFamily = texture.VkRes.GraphicsDevice.GraphicsExecutor.FamilyIndex
        };

        GRBackendRenderTarget backendRenderTarget = new((int)texture.Width, (int)texture.Height, (int)imageInfo.SampleCount, imageInfo);

        return SKSurface.Create(GetOrCreateContext(texture),
                                backendRenderTarget,
                                GRSurfaceOrigin.TopLeft,
                                SKColorType.Rgba8888,
                                SKColorSpace.CreateSrgb());
    }

    private static GRContext GetOrCreateContext(Texture texture)
    {
        if (_cache.TryGetValue(texture.VkRes.VkDevice, out GRContext? context))
        {
            return context;
        }

        GRVkBackendContext backendContext = new()
        {
            VkInstance = texture.VkRes.Instance.Handle,
            VkPhysicalDevice = texture.VkRes.VkPhysicalDevice.Handle,
            VkDevice = texture.VkRes.VkDevice.Handle,
            VkQueue = texture.VkRes.GraphicsDevice.GraphicsExecutor.Handle.Handle,
            GraphicsQueueIndex = texture.VkRes.GraphicsDevice.GraphicsExecutor.FamilyIndex,
            MaxAPIVersion = Context.ApiVersion,
            GetProcedureAddress = GetProcedureAddress
        };

        context = GRContext.CreateVulkan(backendContext);

        _cache[texture.VkRes.VkDevice] = context;

        return context;

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

            return texture.VkRes.Vk.Context.GetProcAddress(name);
        }
    }
}
