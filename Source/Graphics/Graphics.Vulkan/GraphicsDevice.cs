using System.Runtime.CompilerServices;
using Graphics.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Vulkan;

public unsafe class GraphicsDevice : ContextObject
{
    // Staging Resources
    private const uint MinStagingBufferSize = 64;
    private const uint MaxStagingBufferSize = 512;

    private readonly ResourceFactory _resourceFactory;
    private readonly PhysicalDevice _physicalDevice;
    private readonly Device _device;
    private readonly KhrSwapchain _swapchainExt;
    private readonly Queue _graphicsQueue;
    private readonly Queue _computeQueue;
    private readonly Queue _transferQueue;
    private readonly CommandPool _graphicsCommandPool;
    private readonly CommandPool _computeCommandPool;
    private readonly CommandPool _transferCommandPool;
    private readonly DescriptorPoolManager _descriptorPoolManager;
    private readonly SurfaceKHR _windowSurface;
    private readonly PixelFormat _depthFormat;
    private readonly object _stagingResourcesLock;
    private readonly List<DeviceBuffer> _availableStagingBuffers;

    private Swapchain? swapChain;

    internal GraphicsDevice(Context context,
                            PhysicalDevice physicalDevice,
                            Device device,
                            KhrSwapchain swapchainExt,
                            SurfaceKHR windowSurface,
                            uint graphicsQueueFamilyIndex,
                            uint computeQueueFamilyIndex,
                            uint transferQueueFamilyIndex) : base(context)
    {
        Queue graphicsQueue;
        Vk.GetDeviceQueue(device, graphicsQueueFamilyIndex, 0, &graphicsQueue);

        Queue computeQueue;
        Vk.GetDeviceQueue(device, computeQueueFamilyIndex, 0, &computeQueue);

        Queue transferQueue;
        Vk.GetDeviceQueue(device, transferQueueFamilyIndex, 0, &transferQueue);

        CommandPool graphicsCommandPool;
        {
            CommandPoolCreateInfo createInfo = new()
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = graphicsQueueFamilyIndex,
                Flags = CommandPoolCreateFlags.ResetCommandBufferBit
            };

            Vk.CreateCommandPool(device, &createInfo, null, &graphicsCommandPool);
        }

        CommandPool computeCommandPool;
        {
            CommandPoolCreateInfo createInfo = new()
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = computeQueueFamilyIndex,
                Flags = CommandPoolCreateFlags.ResetCommandBufferBit
            };

            Vk.CreateCommandPool(device, &createInfo, null, &computeCommandPool);
        }

        CommandPool transferCommandPool;
        {
            CommandPoolCreateInfo createInfo = new()
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = transferQueueFamilyIndex,
                Flags = CommandPoolCreateFlags.ResetCommandBufferBit
            };

            Vk.CreateCommandPool(device, &createInfo, null, &transferCommandPool);
        }

        Format depthFormat = physicalDevice.FindSupportedFormat([Format.D32SfloatS8Uint, Format.D24UnormS8Uint, Format.D32Sfloat],
                                                                ImageTiling.Optimal,
                                                                FormatFeatureFlags.DepthStencilAttachmentBit);

        _resourceFactory = new ResourceFactory(context, this);
        _physicalDevice = physicalDevice;
        _device = device;
        _swapchainExt = swapchainExt;
        _graphicsQueue = graphicsQueue;
        _computeQueue = computeQueue;
        _transferQueue = transferQueue;
        _graphicsCommandPool = graphicsCommandPool;
        _computeCommandPool = computeCommandPool;
        _transferCommandPool = transferCommandPool;
        _descriptorPoolManager = new DescriptorPoolManager(this);
        _windowSurface = windowSurface;
        _depthFormat = Formats.GetPixelFormat(depthFormat);
        _stagingResourcesLock = new object();
        _availableStagingBuffers = [];
    }

    public ResourceFactory ResourceFactory => _resourceFactory;

    public Swapchain Swapchain => swapChain!;

    internal PhysicalDevice PhysicalDevice => _physicalDevice;

    internal Device Device => _device;

    internal KhrSwapchain SwapchainExt => _swapchainExt;

    internal Queue GraphicsQueue => _graphicsQueue;

    internal Queue ComputeQueue => _computeQueue;

    internal Queue TransferQueue => _transferQueue;

    internal CommandPool GraphicsCommandPool => _graphicsCommandPool;

    internal CommandPool ComputeCommandPool => _computeCommandPool;

    internal CommandPool TransferCommandPool => _transferCommandPool;

    internal DescriptorPoolManager DescriptorPoolManager => _descriptorPoolManager;

    internal CommandBuffer BeginSingleTimeCommands()
    {
        CommandBufferAllocateInfo allocateInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _transferCommandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        CommandBuffer commandBuffer;
        Vk.AllocateCommandBuffers(_device, &allocateInfo, &commandBuffer);

        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        Vk.BeginCommandBuffer(commandBuffer, &beginInfo);

        return commandBuffer;
    }

    internal void EndSingleTimeCommands(CommandBuffer commandBuffer)
    {
        Vk.EndCommandBuffer(commandBuffer);

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer
        };

        Vk.QueueSubmit(_transferQueue, 1, &submitInfo, default);

        Vk.QueueWaitIdle(_transferQueue);

        Vk.FreeCommandBuffers(_device, _transferCommandPool, 1, &commandBuffer);
    }

    public void Resize(uint width, uint height)
    {
        swapChain?.Dispose();

        SwapchainDescription swapchainDescription = new(_windowSurface, width, height, _depthFormat);

        swapChain = new Swapchain(this, in swapchainDescription);
    }

    public void UpdateBuffer(DeviceBuffer buffer, uint bufferOffsetInBytes, void* source, uint sizeInBytes)
    {
        if (bufferOffsetInBytes + sizeInBytes > buffer.SizeInBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeInBytes), "The buffer offset and size exceed the buffer size.");
        }

        if (sizeInBytes == 0)
        {
            return;
        }

        DeviceBuffer stagingBuffer = GetStagingBuffer(sizeInBytes);

        void* stagingBufferPointer = stagingBuffer.DeviceMemory.Map(sizeInBytes);

        Unsafe.CopyBlock(stagingBufferPointer, source, sizeInBytes);

        stagingBuffer.DeviceMemory.Unmap();

        CommandBuffer commandBuffer = BeginSingleTimeCommands();

        BufferCopy bufferCopy = new()
        {
            SrcOffset = 0,
            DstOffset = bufferOffsetInBytes,
            Size = sizeInBytes
        };

        Vk.CmdCopyBuffer(commandBuffer, stagingBuffer.Handle, buffer.Handle, 1, &bufferCopy);

        EndSingleTimeCommands(commandBuffer);

        lock (_stagingResourcesLock)
        {
            if (stagingBuffer.SizeInBytes > MaxStagingBufferSize)
            {
                stagingBuffer.Dispose();
            }
            else
            {
                _availableStagingBuffers.Add(stagingBuffer);
            }
        }
    }

    public void UpdateBuffer<T>(DeviceBuffer buffer, uint bufferOffsetInBytes, T source) where T : unmanaged
    {
        UpdateBuffer(buffer, bufferOffsetInBytes, &source, (uint)sizeof(T));
    }

    public void UpdateBuffer<T>(DeviceBuffer buffer, uint bufferOffsetInBytes, T[] source) where T : unmanaged
    {
        UpdateBuffer(buffer, bufferOffsetInBytes, Unsafe.AsPointer(ref source), (uint)(sizeof(T) * source.Length));
    }

    protected override void Destroy()
    {
        foreach (DeviceBuffer deviceBuffer in _availableStagingBuffers)
        {
            deviceBuffer.Dispose();
        }

        swapChain?.Dispose();

        _descriptorPoolManager.Dispose();

        Vk.DestroyCommandPool(_device, _transferCommandPool, null);
        Vk.DestroyCommandPool(_device, _computeCommandPool, null);
        Vk.DestroyCommandPool(_device, _graphicsCommandPool, null);

        _swapchainExt.Dispose();

        _resourceFactory.Dispose();

        Vk.DestroyDevice(_device, null);
    }

    private DeviceBuffer GetStagingBuffer(uint sizeInBytes)
    {
        lock (_stagingResourcesLock)
        {
            foreach (DeviceBuffer deviceBuffer in _availableStagingBuffers)
            {
                if (deviceBuffer.SizeInBytes >= sizeInBytes)
                {
                    _availableStagingBuffers.Remove(deviceBuffer);

                    return deviceBuffer;
                }
            }
        }

        uint bufferSize = Math.Max(MinStagingBufferSize, sizeInBytes);

        return ResourceFactory.CreateBuffer(new BufferDescription(bufferSize, BufferUsage.Staging));
    }
}

public unsafe partial class Context
{
    public GraphicsDevice CreateGraphicsDevice(PhysicalDevice physicalDevice, Window window)
    {
        float queuePriority = 1.0f;

        uint graphicsQueueFamilyIndex = GetQueueFamilyIndex(physicalDevice, QueueFlags.GraphicsBit);
        uint computeQueueFamilyIndex = GetQueueFamilyIndex(physicalDevice, QueueFlags.ComputeBit);
        uint transferQueueFamilyIndex = GetQueueFamilyIndex(physicalDevice, QueueFlags.TransferBit);

        HashSet<uint> uniqueQueueFamilyIndices =
        [
            graphicsQueueFamilyIndex,
            computeQueueFamilyIndex,
            transferQueueFamilyIndex
        ];

        DeviceQueueCreateInfo[] createInfos = new DeviceQueueCreateInfo[uniqueQueueFamilyIndices.Count];

        for (int i = 0; i < createInfos.Length; i++)
        {
            createInfos[i] = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = uniqueQueueFamilyIndices.ElementAt(i),
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };
        }

        string[] deviceExtensions = [KhrSwapchain.ExtensionName];

        PhysicalDeviceFeatures physicalDeviceFeatures = new()
        {
            SampleRateShading = Vk.True
        };

        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = (uint)createInfos.Length,
            PQueueCreateInfos = _alloter.Allocate(createInfos),
            EnabledExtensionCount = (uint)deviceExtensions.Length,
            PpEnabledExtensionNames = _alloter.Allocate(deviceExtensions),
            PEnabledFeatures = &physicalDeviceFeatures
        };

        Device device;
        _vk.CreateDevice(physicalDevice.VkPhysicalDevice, &createInfo, null, &device).ThrowCode("Failed to create device.");

        KhrSwapchain swapchainExt = CreateDeviceExtension<KhrSwapchain>(device)!;

        SurfaceKHR windowSurface = window.IWindow.VkSurface!.Create<AllocationCallbacks>(_instance.ToHandle(), null).ToSurface();

        _alloter.Clear();

        GraphicsDevice graphicsDevice = new(this,
                                            physicalDevice,
                                            device,
                                            swapchainExt,
                                            windowSurface,
                                            graphicsQueueFamilyIndex,
                                            computeQueueFamilyIndex,
                                            transferQueueFamilyIndex);

        graphicsDevice.Resize((uint)window.Width, (uint)window.Height);

        return graphicsDevice;
    }

    private T? CreateDeviceExtension<T>(Device device) where T : NativeExtension<Vk>
    {
        if (!_vk.TryGetDeviceExtension(_instance, device, out T ext))
        {
            throw new InvalidOperationException($"Failed to load extension {typeof(T).Name}!");
        }

        return ext;
    }

    private static uint GetQueueFamilyIndex(PhysicalDevice physicalDevice, QueueFlags flags)
    {
        for (int i = 0; i < physicalDevice.QueueFamilyProperties.Length; i++)
        {
            if ((physicalDevice.QueueFamilyProperties[i].QueueFlags & flags) == flags)
            {
                return (uint)i;
            }
        }

        return 0;
    }
}
