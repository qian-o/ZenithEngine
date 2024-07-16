using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Vulkan;

public abstract unsafe class DeviceResource(GraphicsDevice graphicsDevice) : ContextObject(graphicsDevice.Context)
{
    private string name = string.Empty;

    public string Name { get => name; set { name = value; UpdateResourceName(); } }

    internal PhysicalDevice PhysicalDevice => graphicsDevice.PhysicalDevice;

    internal VkPhysicalDevice VkPhysicalDevice => graphicsDevice.PhysicalDevice.VkPhysicalDevice;

    internal GraphicsDevice GraphicsDevice => graphicsDevice;

    internal ResourceFactory ResourceFactory => graphicsDevice.ResourceFactory;

    internal DescriptorPoolManager DescriptorPoolManager => graphicsDevice.DescriptorPoolManager;

    internal Device Device => graphicsDevice.Device;

    internal KhrSwapchain SwapchainExt => graphicsDevice.SwapchainExt;

    internal Queue GraphicsQueue => graphicsDevice.GraphicsQueue;

    internal Queue ComputeQueue => graphicsDevice.ComputeQueue;

    internal Queue TransferQueue => graphicsDevice.TransferQueue;

    internal CommandPool GraphicsCommandPool => graphicsDevice.GraphicsCommandPool;

    internal CommandPool ComputeCommandPool => graphicsDevice.ComputeCommandPool;

    internal CommandPool TransferCommandPool => graphicsDevice.TransferCommandPool;

    private void UpdateResourceName()
    {
        switch (this)
        {
            case DeviceMemory deviceMemory:
                {
                    SetDebugMarkerName(ObjectType.DeviceMemory, deviceMemory.Handle.Handle, Name);
                }
                break;
            case DeviceBuffer deviceBuffer:
                {
                    SetDebugMarkerName(ObjectType.Buffer, deviceBuffer.Handle.Handle, Name);
                }
                break;
            case Texture texture:
                {
                    SetDebugMarkerName(ObjectType.Image, texture.Handle.Handle, Name);
                }
                break;
            case TextureView textureView:
                {
                    SetDebugMarkerName(ObjectType.ImageView, textureView.Handle.Handle, Name);
                }
                break;
            case Sampler sampler:
                {
                    SetDebugMarkerName(ObjectType.Sampler, sampler.Handle.Handle, Name);
                }
                break;
            case Framebuffer framebuffer:
                {
                    SetDebugMarkerName(ObjectType.Framebuffer, framebuffer.Handle.Handle, Name);
                }
                break;
            case Shader shader:
                {
                    SetDebugMarkerName(ObjectType.ShaderModule, shader.Handle.Handle, Name);
                }
                break;
            case Pipeline pipeline:
                {
                    SetDebugMarkerName(ObjectType.Pipeline, pipeline.Handle.Handle, Name);
                }
                break;
            case CommandList commandList:
                {
                    SetDebugMarkerName(ObjectType.CommandBuffer, (ulong)commandList.Handle.Handle, Name);
                }
                break;
        }

        Alloter.Clear();
    }

    private void SetDebugMarkerName(ObjectType type, ulong handle, string name)
    {
        DebugUtilsObjectNameInfoEXT nameInfo = new()
        {
            SType = StructureType.DebugUtilsObjectNameInfoExt,
            ObjectType = type,
            ObjectHandle = handle,
            PObjectName = Alloter.Allocate(name)
        };

        DebugUtilsExt?.SetDebugUtilsObjectName(Device, &nameInfo).ThrowCode();
    }
}
