using System.Runtime.CompilerServices;
using Graphics.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Vulkan;

public unsafe class GraphicsDevice : ContextObject
{
    internal const uint MinStagingBufferSize = 1024 * 4;
    internal const uint MaxStagingBufferSize = 1024 * 1024 * 4;
    private readonly object _stagingResourcesLock;
    private readonly List<StagingCommandPool> _availableStagingCommandPools;
    private readonly List<DeviceBuffer> _availableStagingBuffers;
    private readonly List<Semaphore> _availableStagingSemaphores;
    private readonly List<Fence> _availableStagingFences;

    internal GraphicsDevice(Context context,
                            PhysicalDevice physicalDevice,
                            Device device,
                            KhrSwapchain swapchainExt,
                            ExtDescriptorBuffer descriptorBufferExt,
                            IVkSurface windowSurface,
                            uint graphicsQueueFamilyIndex,
                            uint computeQueueFamilyIndex,
                            uint transferQueueFamilyIndex) : base(context)
    {
        PhysicalDevice = physicalDevice;
        Device = device;
        ResourceFactory = new ResourceFactory(context, this);
        SwapchainExt = swapchainExt;
        DescriptorBufferExt = descriptorBufferExt;
        GraphicsQueue = new Queue(this, graphicsQueueFamilyIndex);
        ComputeQueue = new Queue(this, computeQueueFamilyIndex);
        TransferQueue = new Queue(this, transferQueueFamilyIndex);
        GraphicsCommandPool = new CommandPool(this, graphicsQueueFamilyIndex);
        ComputeCommandPool = new CommandPool(this, computeQueueFamilyIndex);
        MainSwapchain = ResourceFactory.CreateSwapchain(new SwapchainDescription(windowSurface, 0, 0, GetBestDepthFormat()));
        PointSampler = ResourceFactory.CreateSampler(SamplerDescription.Point);
        LinearSampler = ResourceFactory.CreateSampler(SamplerDescription.Linear);
        Aniso4xSampler = ResourceFactory.CreateSampler(SamplerDescription.Aniso4x);
        _stagingResourcesLock = new object();
        _availableStagingCommandPools = [];
        _availableStagingBuffers = [];
        _availableStagingSemaphores = [];
        _availableStagingFences = [];
    }

    public ResourceFactory ResourceFactory { get; }

    public Swapchain MainSwapchain { get; }

    public Sampler PointSampler { get; }

    public Sampler LinearSampler { get; }

    public Sampler Aniso4xSampler { get; }

    internal PhysicalDevice PhysicalDevice { get; }

    internal VkDevice Device { get; }

    internal KhrSwapchain SwapchainExt { get; }

    internal ExtDescriptorBuffer DescriptorBufferExt { get; }

    internal Queue GraphicsQueue { get; }

    internal Queue ComputeQueue { get; }

    internal Queue TransferQueue { get; }

    internal CommandPool GraphicsCommandPool { get; }

    internal CommandPool ComputeCommandPool { get; }

    public PixelFormat GetBestDepthFormat()
    {
        Format format = PhysicalDevice.FindSupportedFormat([Format.D32SfloatS8Uint, Format.D24UnormS8Uint, Format.D32Sfloat],
                                                            ImageTiling.Optimal,
                                                            FormatFeatureFlags.DepthStencilAttachmentBit);

        return Formats.GetPixelFormat(format);
    }

    #region UpdateBuffer
    public void UpdateBuffer<T>(DeviceBuffer buffer, uint bufferOffsetInBytes, T* source, int length) where T : unmanaged
    {
        uint sizeInBytes = (uint)(sizeof(T) * length);

        if (bufferOffsetInBytes + sizeInBytes > buffer.SizeInBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferOffsetInBytes), "The buffer offset and size exceed the buffer size.");
        }

        if (sizeInBytes == 0)
        {
            return;
        }

        if (buffer.IsHostVisible)
        {
            void* bufferPointer = buffer.Map(sizeInBytes, bufferOffsetInBytes);

            Unsafe.CopyBlock(bufferPointer, source, sizeInBytes);

            buffer.Unmap();
        }
        else
        {
            StagingCommandPool sharedCommandPool = GetStagingCommandPool();
            DeviceBuffer stagingBuffer = GetStagingBuffer(sizeInBytes);

            void* stagingBufferPointer = stagingBuffer.Map(sizeInBytes);

            Unsafe.CopyBlock(stagingBufferPointer, source, sizeInBytes);

            stagingBuffer.Unmap();

            CommandBuffer commandBuffer = sharedCommandPool.BeginNewCommandBuffer();

            BufferCopy bufferCopy = new()
            {
                SrcOffset = 0,
                DstOffset = bufferOffsetInBytes,
                Size = sizeInBytes
            };

            Vk.CmdCopyBuffer(commandBuffer, stagingBuffer.Handle, buffer.Handle, 1, &bufferCopy);

            sharedCommandPool.EndAndSubmitCommandBuffer(commandBuffer);

            CacheStagingBuffer(stagingBuffer);
            CacheStagingCommandPool(sharedCommandPool);
        }
    }

    public void UpdateBuffer<T>(DeviceBuffer buffer, uint bufferOffsetInBytes, T[] source) where T : unmanaged
    {
        fixed (T* sourcePointer = source)
        {
            UpdateBuffer(buffer, bufferOffsetInBytes, sourcePointer, source.Length);
        }
    }

    public void UpdateBuffer<T>(DeviceBuffer buffer, uint bufferOffsetInBytes, ref readonly T source) where T : unmanaged
    {
        fixed (T* sourcePointer = &source)
        {
            UpdateBuffer(buffer, bufferOffsetInBytes, sourcePointer, 1);
        }
    }
    #endregion

    #region UpdateTexture
    public void UpdateTexture<T>(Texture texture,
                                 T* source,
                                 int length,
                                 uint x,
                                 uint y,
                                 uint z,
                                 uint width,
                                 uint height,
                                 uint depth,
                                 uint mipLevel,
                                 uint arrayLayer) where T : unmanaged
    {
        uint sizeInBytes = (uint)(sizeof(T) * length);

        if (x + width > texture.Width)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "The width exceeds the texture width.");
        }

        if (y + height > texture.Height)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "The height exceeds the texture height.");
        }

        if (z + depth > texture.Depth)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), "The depth exceeds the texture depth.");
        }

        if (mipLevel >= texture.MipLevels)
        {
            throw new ArgumentOutOfRangeException(nameof(mipLevel), "The mip level exceeds the texture mip levels.");
        }

        if (arrayLayer >= texture.ArrayLayers)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayLayer), "The array layer exceeds the texture array layers.");
        }

        if (sizeInBytes == 0)
        {
            return;
        }

        StagingCommandPool sharedCommandPool = GetStagingCommandPool();
        DeviceBuffer stagingBuffer = GetStagingBuffer(sizeInBytes);

        void* stagingBufferPointer = stagingBuffer.Map(sizeInBytes);

        Unsafe.CopyBlock(stagingBufferPointer, source, sizeInBytes);

        stagingBuffer.Unmap();

        CommandBuffer commandBuffer = sharedCommandPool.BeginNewCommandBuffer();

        texture.TransitionLayout(commandBuffer, ImageLayout.TransferDstOptimal);
        {
            BufferImageCopy bufferImageCopy = new()
            {
                BufferOffset = 0,
                BufferRowLength = 0,
                BufferImageHeight = 0,
                ImageSubresource = new ImageSubresourceLayers
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    MipLevel = mipLevel,
                    BaseArrayLayer = arrayLayer,
                    LayerCount = 1
                },
                ImageOffset = new Offset3D((int)x, (int)y, (int)z),
                ImageExtent = new Extent3D(width, height, depth)
            };

            Vk.CmdCopyBufferToImage(commandBuffer, stagingBuffer.Handle, texture.Handle, ImageLayout.TransferDstOptimal, 1, &bufferImageCopy);
        }
        texture.TransitionToBestLayout(commandBuffer);

        sharedCommandPool.EndAndSubmitCommandBuffer(commandBuffer);

        CacheStagingBuffer(stagingBuffer);
        CacheStagingCommandPool(sharedCommandPool);
    }

    public void UpdateTexture<T>(Texture texture,
                                 T[] source,
                                 uint x,
                                 uint y,
                                 uint z,
                                 uint width,
                                 uint height,
                                 uint depth,
                                 uint mipLevel,
                                 uint arrayLayer) where T : unmanaged
    {
        fixed (T* sourcePointer = source)
        {
            UpdateTexture(texture, sourcePointer, source.Length, x, y, z, width, height, depth, mipLevel, arrayLayer);
        }
    }
    #endregion

    public void SubmitCommands(CommandList commandList)
    {
        Fence stagingFence = GetStagingFence();

        SubmitCommandsCore(commandList, null, stagingFence);

        stagingFence.WaitAndReset();

        commandList.Submitted();

        CacheStagingFence(stagingFence);
    }

    public void SwapBuffers(Swapchain swapchain)
    {
        SwapBuffersCore(swapchain, null, true);
    }

    public void SwapBuffers()
    {
        if (MainSwapchain == null)
        {
            throw new InvalidOperationException("The swap chain is not initialized.");
        }

        SwapBuffers(MainSwapchain);
    }

    public void SubmitCommandsAndSwapBuffers(CommandList commandList, Swapchain swapchain)
    {
        Semaphore stagingSemaphore = GetStagingSemaphore();
        Fence stagingFence = GetStagingFence();

        SubmitCommandsCore(commandList, stagingSemaphore, stagingFence);

        Fence[] fences = SwapBuffersCore(swapchain,
                                         stagingSemaphore,
                                         false) ? [stagingFence, swapchain.ImageAvailableFence] : [stagingFence];

        WaitAndResetFences(fences);

        commandList.Submitted();

        CacheStagingFence(stagingFence);
        CacheStagingSemaphore(stagingSemaphore);
    }

    protected override void Destroy()
    {
        foreach (Fence fence in _availableStagingFences)
        {
            fence.Dispose();
        }

        foreach (Semaphore semaphore in _availableStagingSemaphores)
        {
            semaphore.Dispose();
        }

        foreach (DeviceBuffer deviceBuffer in _availableStagingBuffers)
        {
            deviceBuffer.Dispose();
        }

        foreach (StagingCommandPool sharedCommandPool in _availableStagingCommandPools)
        {
            sharedCommandPool.Dispose();
        }

        _availableStagingFences.Clear();
        _availableStagingSemaphores.Clear();
        _availableStagingBuffers.Clear();
        _availableStagingCommandPools.Clear();

        Aniso4xSampler.Dispose();
        LinearSampler.Dispose();
        PointSampler.Dispose();

        MainSwapchain.Dispose();

        ComputeCommandPool.Dispose();
        GraphicsCommandPool.Dispose();

        SwapchainExt.Dispose();

        ResourceFactory.Dispose();

        Vk.DestroyDevice(Device, null);
    }

    private void SubmitCommandsCore(CommandList commandList, Semaphore? signalSemaphore, Fence fence)
    {
        CommandBuffer commandBuffer = commandList.Handle;
        Queue queue = commandList.Queue;

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer
        };

        if (signalSemaphore != null)
        {
            VkSemaphore signalSemaphoreHandle = signalSemaphore.Handle;

            submitInfo.SignalSemaphoreCount = 1;
            submitInfo.PSignalSemaphores = &signalSemaphoreHandle;
        }

        Vk.QueueSubmit(queue.Handle, 1, &submitInfo, fence.Handle);
    }

    private bool SwapBuffersCore(Swapchain swapchain, Semaphore? waitSemaphore, bool waitAcquireNextImage)
    {
        SwapchainKHR swapchainKHR = swapchain.Handle;
        uint imageIndex = swapchain.CurrentImageIndex;

        PresentInfoKHR presentInfo = new()
        {
            SType = StructureType.PresentInfoKhr,
            SwapchainCount = 1,
            PSwapchains = &swapchainKHR,
            PImageIndices = &imageIndex
        };

        if (waitSemaphore != null)
        {
            VkSemaphore waitSemaphoreHandle = waitSemaphore.Handle;

            presentInfo.WaitSemaphoreCount = 1;
            presentInfo.PWaitSemaphores = &waitSemaphoreHandle;
        }

        Result result = SwapchainExt.QueuePresent(GraphicsQueue.Handle, &presentInfo);

        if (result == Result.ErrorOutOfDateKhr)
        {
            return false;
        }

        if (result is not Result.Success and not Result.SuboptimalKhr)
        {
            result.ThrowCode("Failed to present the swap chain image.");
        }

        swapchain.AcquireNextImage(waitAcquireNextImage);

        return true;
    }

    private void WaitAndResetFences(Fence[] fences)
    {
        VkFence[] vkFences = fences.Select(f => f.Handle).ToArray();

        fixed (VkFence* vkFencesPointer = vkFences)
        {
            Vk.WaitForFences(Device, (uint)vkFences.Length, vkFencesPointer, Vk.True, ulong.MaxValue);
            Vk.ResetFences(Device, (uint)vkFences.Length, vkFencesPointer);
        }
    }

    private StagingCommandPool GetStagingCommandPool()
    {
        lock (_stagingResourcesLock)
        {
            foreach (StagingCommandPool stagingCommandPool in _availableStagingCommandPools)
            {
                _availableStagingCommandPools.Remove(stagingCommandPool);

                return stagingCommandPool;
            }
        }

        return new StagingCommandPool(this, TransferQueue);
    }

    private void CacheStagingCommandPool(StagingCommandPool stagingCommandPool)
    {
        lock (_stagingResourcesLock)
        {
            _availableStagingCommandPools.Add(stagingCommandPool);
        }
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

    private void CacheStagingBuffer(DeviceBuffer stagingBuffer)
    {
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

    private Semaphore GetStagingSemaphore()
    {
        lock (_stagingResourcesLock)
        {
            foreach (Semaphore stagingSemaphore in _availableStagingSemaphores)
            {
                _availableStagingSemaphores.Remove(stagingSemaphore);

                return stagingSemaphore;
            }
        }

        return new Semaphore(this);
    }

    private void CacheStagingSemaphore(Semaphore stagingSemaphore)
    {
        lock (_stagingResourcesLock)
        {
            _availableStagingSemaphores.Add(stagingSemaphore);
        }
    }

    private Fence GetStagingFence()
    {
        lock (_stagingResourcesLock)
        {
            foreach (Fence stagingFence in _availableStagingFences)
            {
                _availableStagingFences.Remove(stagingFence);

                return stagingFence;
            }
        }

        return new Fence(this);
    }

    private void CacheStagingFence(Fence stagingFence)
    {
        lock (_stagingResourcesLock)
        {
            _availableStagingFences.Add(stagingFence);
        }
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

        string[] deviceExtensions = [KhrSwapchain.ExtensionName, ExtDescriptorBuffer.ExtensionName];

        PhysicalDeviceDescriptorIndexingFeatures descriptorIndexingFeatures = new()
        {
            SType = StructureType.PhysicalDeviceDescriptorIndexingFeatures
        };
        PhysicalDeviceDescriptorBufferFeaturesEXT descriptorBufferFeaturesEXT = new()
        {
            SType = StructureType.PhysicalDeviceDescriptorBufferFeaturesExt,
            PNext = &descriptorIndexingFeatures
        };
        PhysicalDeviceBufferDeviceAddressFeatures bufferDeviceAddressFeatures = new()
        {
            SType = StructureType.PhysicalDeviceBufferDeviceAddressFeatures,
            PNext = &descriptorBufferFeaturesEXT
        };
        PhysicalDeviceVulkan13Features vulkan13Features = new()
        {
            SType = StructureType.PhysicalDeviceVulkan13Features,
            PNext = &bufferDeviceAddressFeatures
        };
        PhysicalDeviceFeatures2 features2 = new()
        {
            SType = StructureType.PhysicalDeviceFeatures2,
            PNext = &vulkan13Features
        };
        Vk.GetPhysicalDeviceFeatures2(physicalDevice.Handle, &features2);

        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = (uint)createInfos.Length,
            PQueueCreateInfos = Alloter.Allocate(createInfos),
            EnabledExtensionCount = (uint)deviceExtensions.Length,
            PpEnabledExtensionNames = Alloter.Allocate(deviceExtensions),
            PNext = &features2
        };

        Device device;
        Vk.CreateDevice(physicalDevice.Handle, &createInfo, null, &device).ThrowCode("Failed to create device.");

        KhrSwapchain swapchainExt = CreateDeviceExtension<KhrSwapchain>(device);
        ExtDescriptorBuffer descriptorBufferExt = CreateDeviceExtension<ExtDescriptorBuffer>(device);

        Alloter.Clear();

        GraphicsDevice graphicsDevice = new(this,
                                            physicalDevice,
                                            device,
                                            swapchainExt,
                                            descriptorBufferExt,
                                            window.VkSurface!,
                                            graphicsQueueFamilyIndex,
                                            computeQueueFamilyIndex,
                                            transferQueueFamilyIndex);

        graphicsDevice.MainSwapchain.Resize((uint)window.FramebufferSize.X, (uint)window.FramebufferSize.Y);

        return graphicsDevice;
    }

    private T CreateDeviceExtension<T>(Device device) where T : NativeExtension<Vk>
    {
        if (!Vk.TryGetDeviceExtension(Instance, device, out T ext))
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
