﻿using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Vulkan;

public abstract class DeviceResource(GraphicsDevice graphicsDevice) : ContextObject(graphicsDevice.Context)
{
    private string name = string.Empty;

    public string Name { get => name; set => name = value; }

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
}
