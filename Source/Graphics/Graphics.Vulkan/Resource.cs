using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Vulkan;

public abstract class Resource(GraphicsDevice graphicsDevice) : ContextObject(graphicsDevice.Context)
{
    internal GraphicsDevice GraphicsDevice => graphicsDevice;

    internal ResourceFactory ResourceFactory => graphicsDevice.ResourceFactory;

    internal VkPhysicalDevice PhysicalDevice => graphicsDevice.PhysicalDevice.VkPhysicalDevice;

    internal Device Device => graphicsDevice.Device;

    internal KhrSwapchain SwapchainExt => graphicsDevice.SwapchainExt;

    internal SurfaceKHR WindowSurface => graphicsDevice.WindowSurface;

    internal Queue GraphicsQueue => graphicsDevice.GraphicsQueue;

    internal Queue ComputeQueue => graphicsDevice.ComputeQueue;

    internal Queue TransferQueue => graphicsDevice.TransferQueue;

    internal CommandPool GraphicsCommandPool => graphicsDevice.GraphicsCommandPool;

    internal CommandPool ComputeCommandPool => graphicsDevice.ComputeCommandPool;

    internal CommandPool TransferCommandPool => graphicsDevice.TransferCommandPool;
}
