using System.Runtime.CompilerServices;
using Graphics.Core;
using Silk.NET.Vulkan;
using Viewport = Graphics.Core.Viewport;

namespace Graphics.Vulkan;

public unsafe class CommandList : VulkanObject<CommandBuffer>
{
    private readonly object _disposablesLock;
    private readonly List<DisposableObject> _disposables;
    private readonly object _stagingResourcesLock;
    private readonly List<DeviceBuffer> _availableStagingBuffers;
    private readonly List<DeviceBuffer> _usedStagingBuffers;

    private bool _isRecording;
    private Framebuffer? _currentFramebuffer;
    private Pipeline? _currentPipeline;
    private bool _isInRenderPass;

    internal CommandList(VulkanResources vkRes, Executor taskExecutor, CommandPool commandPool) : base(vkRes, ObjectType.CommandBuffer)
    {
        Handle = commandPool.AllocateCommandBuffer();
        TaskExecutor = taskExecutor;
        CommandPool = commandPool;

        _disposablesLock = new();
        _disposables = [];
        _stagingResourcesLock = new();
        _availableStagingBuffers = [];
        _usedStagingBuffers = [];
    }

    internal override CommandBuffer Handle { get; }

    internal Executor TaskExecutor { get; }

    internal CommandPool CommandPool { get; }

    public void Begin()
    {
        if (_isRecording)
        {
            throw new InvalidOperationException("Command list is already recording.");
        }

        VkRes.Vk.ResetCommandBuffer(Handle, CommandBufferResetFlags.None).ThrowCode();

        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        VkRes.Vk.BeginCommandBuffer(Handle, &beginInfo).ThrowCode();

        _isRecording = true;
    }

    public void SetFramebuffer(Framebuffer framebuffer)
    {
        EndRenderPass();

        _currentFramebuffer = framebuffer;

        BeginRenderPass();

        SetFullViewports();
        SetFullScissorRects();
    }

    public void SetViewport(uint index, Viewport viewport)
    {
        float y = viewport.Y + viewport.Height;
        float height = -viewport.Height;

        VkViewport vkViewport = new()
        {
            X = viewport.X,
            Y = y,
            Width = viewport.Width,
            Height = height,
            MinDepth = viewport.MinDepth,
            MaxDepth = viewport.MaxDepth
        };

        VkRes.Vk.CmdSetViewport(Handle, index, 1, &vkViewport);
    }

    public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
    {
        Rect2D scissor = new()
        {
            Offset = new Offset2D { X = (int)x, Y = (int)y },
            Extent = new Extent2D { Width = width, Height = height }
        };

        VkRes.Vk.CmdSetScissor(Handle, index, 1, &scissor);
    }

    public void SetFullViewports()
    {
        if (_currentFramebuffer == null)
        {
            throw new InvalidOperationException("No framebuffer set.");
        }

        for (uint i = 0; i < _currentFramebuffer.ColorAttachmentCount; i++)
        {
            SetViewport(i, new Viewport(0, 0, _currentFramebuffer.Width, _currentFramebuffer.Height, 0, 1));
        }
    }

    public void SetFullScissorRects()
    {
        if (_currentFramebuffer == null)
        {
            throw new InvalidOperationException("No framebuffer set.");
        }

        for (uint i = 0; i < _currentFramebuffer.ColorAttachmentCount; i++)
        {
            SetScissorRect(i, 0, 0, _currentFramebuffer.Width, _currentFramebuffer.Height);
        }
    }

    public void ClearColorTarget(uint index, RgbaFloat clearColor)
    {
        if (_currentFramebuffer == null)
        {
            throw new InvalidOperationException("No framebuffer set.");
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _currentFramebuffer.ColorAttachmentCount);

        ClearAttachment clearAttachment = new()
        {
            AspectMask = ImageAspectFlags.ColorBit,
            ColorAttachment = index,
            ClearValue = new ClearValue { Color = new ClearColorValue(clearColor.R, clearColor.G, clearColor.B, clearColor.A) }
        };

        ClearRect clearRect = new()
        {
            BaseArrayLayer = 0,
            LayerCount = 1,
            Rect = new Rect2D
            {
                Offset = new Offset2D(),
                Extent = new Extent2D
                {
                    Width = _currentFramebuffer.Width,
                    Height = _currentFramebuffer.Height
                }
            }
        };

        VkRes.Vk.CmdClearAttachments(Handle, 1, &clearAttachment, 1, &clearRect);
    }

    public void ClearDepthStencil(float depth, byte stencil)
    {
        if (_currentFramebuffer == null)
        {
            throw new InvalidOperationException("No framebuffer set.");
        }

        if (_currentFramebuffer.DepthAttachmentCount == 0)
        {
            throw new InvalidOperationException("Framebuffer does not have a depth attachment.");
        }

        ClearAttachment clearAttachment = new()
        {
            AspectMask = ImageAspectFlags.DepthBit | ImageAspectFlags.StencilBit,
            ClearValue = new ClearValue { DepthStencil = new ClearDepthStencilValue { Depth = depth, Stencil = stencil } }
        };

        ClearRect clearRect = new()
        {
            BaseArrayLayer = 0,
            LayerCount = 1,
            Rect = new Rect2D
            {
                Offset = new Offset2D(),
                Extent = new Extent2D
                {
                    Width = _currentFramebuffer.Width,
                    Height = _currentFramebuffer.Height
                }
            }
        };

        VkRes.Vk.CmdClearAttachments(Handle, 1, &clearAttachment, 1, &clearRect);
    }

    public void ClearDepthStencil(float depth)
    {
        ClearDepthStencil(depth, 0);
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

        EnsureRenderPassInactive();

        DeviceBuffer stagingBuffer = GetStagingBuffer(sizeInBytes);

        void* stagingBufferPointer = stagingBuffer.Map(sizeInBytes);

        Unsafe.CopyBlock(stagingBufferPointer, source, sizeInBytes);

        stagingBuffer.Unmap();

        // Copy the staging buffer to the target buffer
        {
            BufferCopy bufferCopy = new()
            {
                SrcOffset = 0,
                DstOffset = bufferOffsetInBytes,
                Size = sizeInBytes
            };

            VkRes.Vk.CmdCopyBuffer(Handle, stagingBuffer.Handle, buffer.Handle, 1, &bufferCopy);
        }

        // Add a memory barrier to ensure that the buffer is ready to be used
        {
            bool needToProtectUniformBuffer = buffer.Usage.HasFlag(BufferUsage.UniformBuffer);

            MemoryBarrier memoryBarrier = new()
            {
                SType = StructureType.MemoryBarrier,
                SrcAccessMask = AccessFlags.MemoryWriteBit,
                DstAccessMask = needToProtectUniformBuffer ? AccessFlags.UniformReadBit : AccessFlags.VertexAttributeReadBit
            };

            VkRes.Vk.CmdPipelineBarrier(Handle,
                                        PipelineStageFlags.TransferBit,
                                        needToProtectUniformBuffer ? PipelineStageFlags.AllGraphicsBit : PipelineStageFlags.VertexInputBit,
                                        DependencyFlags.None,
                                        1,
                                        &memoryBarrier,
                                        0,
                                        null,
                                        0,
                                        null);
        }

        RecordUsedStagingBuffer(stagingBuffer);
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

        EnsureRenderPassInactive();

        DeviceBuffer stagingBuffer = GetStagingBuffer(sizeInBytes);

        void* stagingBufferPointer = stagingBuffer.Map(sizeInBytes);

        Unsafe.CopyBlock(stagingBufferPointer, source, sizeInBytes);

        stagingBuffer.Unmap();

        texture.TransitionLayout(Handle, ImageLayout.TransferDstOptimal);
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

            VkRes.Vk.CmdCopyBufferToImage(Handle, stagingBuffer.Handle, texture.Handle, ImageLayout.TransferDstOptimal, 1, &bufferImageCopy);
        }
        texture.TransitionToBestLayout(Handle);

        RecordUsedStagingBuffer(stagingBuffer);
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

    public void SetVertexBuffer(uint index, DeviceBuffer buffer, uint offset)
    {
        VkBuffer vkBuffer = buffer.Handle;
        ulong vkOffset = offset;

        VkRes.Vk.CmdBindVertexBuffers(Handle, index, 1, &vkBuffer, &vkOffset);
    }

    public void SetVertexBuffer(uint index, DeviceBuffer buffer)
    {
        SetVertexBuffer(index, buffer, 0);
    }

    public void SetIndexBuffer(DeviceBuffer buffer, IndexFormat format, uint offset)
    {
        VkRes.Vk.CmdBindIndexBuffer(Handle, buffer.Handle, offset, Formats.GetIndexType(format));
    }

    public void SetIndexBuffer(DeviceBuffer buffer, IndexFormat format)
    {
        SetIndexBuffer(buffer, format, 0);
    }

    public void SetPipeline(Pipeline pipeline)
    {
        if (pipeline.IsGraphics)
        {
            VkRes.Vk.CmdBindPipeline(Handle, PipelineBindPoint.Graphics, pipeline.Handle);
        }

        _currentPipeline = pipeline;
    }

    public void SetResourceSet(uint slot, ResourceSet resourceSet)
    {
        if (_currentPipeline == null)
        {
            throw new InvalidOperationException("No pipeline set.");
        }

        DescriptorBufferBindingInfoEXT bindingInfoEXT = new()
        {
            SType = StructureType.DescriptorBufferBindingInfoExt,
            Address = resourceSet.Handle.Address,
            Usage = BufferUsageFlags.ResourceDescriptorBufferBitExt | BufferUsageFlags.SamplerDescriptorBufferBitExt
        };

        VkRes.GetExtDescriptorBuffer().CmdBindDescriptorBuffers(Handle, 1, &bindingInfoEXT);

        uint bufferIndices = 0;
        ulong offsets = 0;
        if (_currentPipeline.IsGraphics)
        {
            VkRes.GetExtDescriptorBuffer().CmdSetDescriptorBufferOffsets(Handle,
                                                                         PipelineBindPoint.Graphics,
                                                                         _currentPipeline.Layout,
                                                                         slot,
                                                                         1,
                                                                         &bufferIndices,
                                                                         &offsets);
        }
    }

    public void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
    {
        EnsureRenderPassActive();

        VkRes.Vk.CmdDrawIndexed(Handle, indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
    }

    public void DrawIndexed(uint indexCount)
    {
        DrawIndexed(indexCount, 1, 0, 0, 0);
    }

    public void ResolveTexture(Texture source, Texture destination)
    {
        if (source.SampleCount == TextureSampleCount.Count1)
        {
            throw new InvalidOperationException("Source texture must be multisampled.");
        }

        if (destination.SampleCount != TextureSampleCount.Count1)
        {
            throw new InvalidOperationException("Destination texture must not be multisampled.");
        }

        EnsureRenderPassInactive();

        source.TransitionLayout(Handle, ImageLayout.TransferSrcOptimal);
        destination.TransitionLayout(Handle, ImageLayout.TransferDstOptimal);

        ImageAspectFlags aspectMask = source.Usage.HasFlag(TextureUsage.DepthStencil)
            ? ImageAspectFlags.DepthBit | ImageAspectFlags.StencilBit
            : ImageAspectFlags.ColorBit;

        ImageResolve resolve = new()
        {
            Extent = new Extent3D
            {
                Width = source.Width,
                Height = source.Height,
                Depth = source.Depth
            },
            SrcSubresource = new ImageSubresourceLayers
            {
                AspectMask = aspectMask,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            SrcOffset = new Offset3D(),
            DstSubresource = new ImageSubresourceLayers
            {
                AspectMask = aspectMask,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            DstOffset = new Offset3D()
        };

        VkRes.Vk.CmdResolveImage(Handle,
                                 source.Handle,
                                 ImageLayout.TransferSrcOptimal,
                                 destination.Handle,
                                 ImageLayout.TransferDstOptimal,
                                 1,
                                 &resolve);

        source.TransitionToBestLayout(Handle);
        destination.TransitionToBestLayout(Handle);
    }

    public void GenerateMipmaps(Texture texture)
    {
        if (!texture.Usage.HasFlag(TextureUsage.GenerateMipmaps))
        {
            throw new InvalidOperationException("Texture does not support mipmap generation.");
        }

        if (texture.MipLevels > 1)
        {
            EnsureRenderPassInactive();

            uint width = texture.Width;
            uint height = texture.Height;
            uint depth = texture.Depth;
            uint mipLevels = texture.MipLevels;
            uint arrayLayers = texture.ArrayLayers;

            for (uint level = 1; level < mipLevels; level++)
            {
                texture.TransitionLayout(Handle, level - 1, 1, 0, arrayLayers, ImageLayout.TransferSrcOptimal);
                texture.TransitionLayout(Handle, level, 1, 0, arrayLayers, ImageLayout.TransferDstOptimal);

                uint mipWidth = Math.Max(1, width >> 1);
                uint mipHeight = Math.Max(1, height >> 1);
                uint mipDepth = Math.Max(1, depth >> 1);

                ImageBlit blit = new()
                {
                    SrcOffsets = new ImageBlit.SrcOffsetsBuffer()
                    {
                        Element0 = new Offset3D { X = 0, Y = 0, Z = 0 },
                        Element1 = new Offset3D { X = (int)width, Y = (int)height, Z = (int)depth }
                    },
                    SrcSubresource = new ImageSubresourceLayers
                    {
                        AspectMask = ImageAspectFlags.ColorBit,
                        MipLevel = level - 1,
                        BaseArrayLayer = 0,
                        LayerCount = arrayLayers
                    },
                    DstOffsets = new ImageBlit.DstOffsetsBuffer()
                    {
                        Element0 = new Offset3D { X = 0, Y = 0, Z = 0 },
                        Element1 = new Offset3D { X = (int)mipWidth, Y = (int)mipHeight, Z = (int)mipDepth }
                    },
                    DstSubresource = new ImageSubresourceLayers
                    {
                        AspectMask = ImageAspectFlags.ColorBit,
                        MipLevel = level,
                        BaseArrayLayer = 0,
                        LayerCount = arrayLayers
                    }
                };

                VkRes.Vk.CmdBlitImage(Handle,
                                      texture.Handle,
                                      ImageLayout.TransferSrcOptimal,
                                      texture.Handle,
                                      ImageLayout.TransferDstOptimal,
                                      1,
                                      &blit,
                                      Filter.Linear);

                width = mipWidth;
                height = mipHeight;
                depth = mipDepth;
            }

            texture.TransitionToBestLayout(Handle);
        }
    }

    public void End()
    {
        if (!_isRecording)
        {
            throw new InvalidOperationException("Command list is not recording");
        }

        EndRenderPass();

        VkRes.Vk.EndCommandBuffer(Handle).ThrowCode();

        _isRecording = false;
    }

    public void DisposeSubmitted(DisposableObject disposable)
    {
        lock (_disposablesLock)
        {
            _disposables.Add(disposable);
        }
    }

    internal void Submitted()
    {
        lock (_disposablesLock)
        {
            foreach (DisposableObject disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
        }

        ReturnUsedStagingResources();
    }

    internal override ulong[] GetHandles()
    {
        return [(ulong)Handle.Handle];
    }

    protected override void Destroy()
    {
        ReturnUsedStagingResources();

        foreach (DeviceBuffer deviceBuffer in _availableStagingBuffers)
        {
            deviceBuffer.Dispose();
        }

        _availableStagingBuffers.Clear();

        CommandPool.FreeCommandBuffer(Handle);
    }

    private void BeginRenderPass()
    {
        if (_currentFramebuffer == null)
        {
            return;
        }

        _currentFramebuffer.TransitionToInitialLayout(Handle);

        EnsureRenderPassActive(true);
    }

    private void EndRenderPass()
    {
        if (_currentFramebuffer == null)
        {
            return;
        }

        EnsureRenderPassInactive();

        _currentFramebuffer.TransitionToFinalLayout(Handle);
    }

    private void EnsureRenderPassActive(bool useClearRenderPass = false)
    {
        if (!_isInRenderPass)
        {
            ClearColorValue[] clearColorValues = new ClearColorValue[_currentFramebuffer!.AttachmentCount];
            for (int i = 0; i < clearColorValues.Length; i++)
            {
                clearColorValues[i] = new ClearColorValue(0, 0, 0, 0);
            }

            if (_currentFramebuffer.DepthAttachmentCount != 0)
            {
                clearColorValues[^1] = new ClearColorValue(1, 0);
            }

            RenderPassBeginInfo beginInfo = new()
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = useClearRenderPass ? _currentFramebuffer.RenderPassClear : _currentFramebuffer.RenderPassLoad,
                Framebuffer = _currentFramebuffer.Handle,
                RenderArea = new Rect2D
                {
                    Offset = new Offset2D(),
                    Extent = new Extent2D
                    {
                        Width = _currentFramebuffer.Width,
                        Height = _currentFramebuffer.Height
                    }
                },
                ClearValueCount = (uint)clearColorValues.Length,
                PClearValues = (ClearValue*)clearColorValues.AsPointer()
            };

            VkRes.Vk.CmdBeginRenderPass(Handle, &beginInfo, SubpassContents.Inline);

            _isInRenderPass = true;
        }
    }

    private void EnsureRenderPassInactive()
    {
        if (_isInRenderPass)
        {
            VkRes.Vk.CmdEndRenderPass(Handle);

            VkRes.Vk.CmdPipelineBarrier(Handle,
                                        PipelineStageFlags.BottomOfPipeBit,
                                        PipelineStageFlags.TopOfPipeBit,
                                        DependencyFlags.None,
                                        0,
                                        null,
                                        0,
                                        null,
                                        0,
                                        null);

            _isInRenderPass = false;
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

        uint bufferSize = Math.Max(GraphicsDevice.MinStagingBufferSize, sizeInBytes);

        return VkRes.GraphicsDevice.Factory.CreateBuffer(new BufferDescription(bufferSize, BufferUsage.Staging));
    }

    private void RecordUsedStagingBuffer(DeviceBuffer stagingBuffer)
    {
        lock (_stagingResourcesLock)
        {
            _usedStagingBuffers.Add(stagingBuffer);
        }
    }

    private void CacheStagingBuffer(DeviceBuffer stagingBuffer)
    {
        lock (_stagingResourcesLock)
        {
            if (stagingBuffer.SizeInBytes > GraphicsDevice.MaxStagingBufferSize)
            {
                stagingBuffer.Dispose();
            }
            else
            {
                _availableStagingBuffers.Add(stagingBuffer);
            }
        }
    }

    private void ReturnUsedStagingResources()
    {
        lock (_stagingResourcesLock)
        {
            foreach (DeviceBuffer deviceBuffer in _usedStagingBuffers)
            {
                CacheStagingBuffer(deviceBuffer);
            }

            _usedStagingBuffers.Clear();
        }
    }
}
