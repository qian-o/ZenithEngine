using System.Runtime.CompilerServices;
using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class CommandList : DeviceResource
{
    private readonly Queue _queue;
    private readonly Fence _fence;
    private readonly CommandPool _commandPool;
    private readonly CommandBuffer _commandBuffer;

    private bool _isRecording;
    private Framebuffer? _currentFramebuffer;
    private Pipeline? _currentPipeline;
    private bool _isInRenderPass;

    internal CommandList(GraphicsDevice graphicsDevice, Queue queue, Fence fence, CommandPool commandPool) : base(graphicsDevice)
    {
        CommandBufferAllocateInfo allocateInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        CommandBuffer commandBuffer;
        Vk.AllocateCommandBuffers(Device, &allocateInfo, &commandBuffer).ThrowCode();

        _queue = queue;
        _fence = fence;
        _commandPool = commandPool;
        _commandBuffer = commandBuffer;
    }

    internal CommandBuffer Handle => _commandBuffer;

    internal Queue Queue => _queue;

    internal Fence Fence => _fence;

    public void Begin()
    {
        if (_isRecording)
        {
            throw new InvalidOperationException("Command list is already recording.");
        }

        Vk.ResetCommandBuffer(_commandBuffer, CommandBufferResetFlags.None).ThrowCode();

        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        Vk.BeginCommandBuffer(_commandBuffer, &beginInfo).ThrowCode();

        _isRecording = true;
    }

    public void SetFramebuffer(Framebuffer framebuffer)
    {
        if (_isInRenderPass)
        {
            EndCurrentRenderPass();
        }

        _currentFramebuffer = framebuffer;

        BeginCurrentRenderPass();

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

        Vk.CmdSetViewport(_commandBuffer, index, 1, &vkViewport);
    }

    public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
    {
        Rect2D scissor = new()
        {
            Offset = new Offset2D { X = (int)x, Y = (int)y },
            Extent = new Extent2D { Width = width, Height = height }
        };

        Vk.CmdSetScissor(_commandBuffer, index, 1, &scissor);
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

        Vk.CmdClearAttachments(_commandBuffer, 1, &clearAttachment, 1, &clearRect);
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

        Vk.CmdClearAttachments(_commandBuffer, 1, &clearAttachment, 1, &clearRect);
    }

    public void ClearDepthStencil(float depth)
    {
        ClearDepthStencil(depth, 0);
    }

    public void SetVertexBuffer(uint index, DeviceBuffer buffer, uint offset)
    {
        VkBuffer vkBuffer = buffer.Handle;
        ulong vkOffset = offset;

        Vk.CmdBindVertexBuffers(_commandBuffer, index, 1, &vkBuffer, &vkOffset);
    }

    public void SetVertexBuffer(uint index, DeviceBuffer buffer)
    {
        SetVertexBuffer(index, buffer, 0);
    }

    public void SetIndexBuffer(DeviceBuffer buffer, IndexFormat format, uint offset)
    {
        Vk.CmdBindIndexBuffer(_commandBuffer, buffer.Handle, offset, Formats.GetIndexType(format));
    }

    public void SetIndexBuffer(DeviceBuffer buffer, IndexFormat format)
    {
        SetIndexBuffer(buffer, format, 0);
    }

    public void SetPipeline(Pipeline pipeline)
    {
        if (pipeline.IsGraphics)
        {
            Vk.CmdBindPipeline(_commandBuffer, PipelineBindPoint.Graphics, pipeline.Handle);
        }

        _currentPipeline = pipeline;
    }

    public void SetGraphicsResourceSet(uint slot, ResourceSet resourceSet, uint dynamicOffsetsCount, ref uint dynamicOffsets)
    {
        if (_currentPipeline == null)
        {
            throw new InvalidOperationException("No pipeline set.");
        }

        VkDescriptorSet descriptorSet = resourceSet.Handle;

        Vk.CmdBindDescriptorSets(_commandBuffer,
                                 PipelineBindPoint.Graphics,
                                 _currentPipeline.Layout,
                                 slot,
                                 1,
                                 &descriptorSet,
                                 dynamicOffsetsCount,
                                 ref dynamicOffsets);
    }

    public void SetGraphicsResourceSet(uint slot, ResourceSet resourceSet)
    {
        SetGraphicsResourceSet(slot, resourceSet, 0, ref Unsafe.AsRef<uint>(null));
    }

    public void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
    {
        Vk.CmdDrawIndexed(_commandBuffer, indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
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

        if (_isInRenderPass)
        {
            EndCurrentRenderPass();
        }

        source.TransitionLayout(_commandBuffer, ImageLayout.TransferSrcOptimal);
        destination.TransitionLayout(_commandBuffer, ImageLayout.TransferDstOptimal);

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

        Vk.CmdResolveImage(_commandBuffer,
                           source.Handle,
                           ImageLayout.TransferSrcOptimal,
                           destination.Handle,
                           ImageLayout.TransferDstOptimal,
                           1,
                           &resolve);

        source.TransitionToBestLayout(_commandBuffer);
        destination.TransitionToBestLayout(_commandBuffer);
    }

    public void End()
    {
        if (!_isRecording)
        {
            throw new InvalidOperationException("Command list is not recording");
        }

        if (_isInRenderPass)
        {
            EndCurrentRenderPass();
        }

        Vk.EndCommandBuffer(_commandBuffer).ThrowCode();

        _isRecording = false;
    }

    protected override void Destroy()
    {
        Vk.FreeCommandBuffers(Device, _commandPool, 1, [_commandBuffer]);
    }

    private void BeginCurrentRenderPass()
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
            RenderPass = _currentFramebuffer.RenderPass,
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
            PClearValues = (ClearValue*)Unsafe.AsPointer(ref clearColorValues[0])
        };

        Vk.CmdBeginRenderPass(_commandBuffer, &beginInfo, SubpassContents.Inline);

        _isInRenderPass = true;
    }

    private void EndCurrentRenderPass()
    {
        Vk.CmdEndRenderPass(_commandBuffer);

        Vk.CmdPipelineBarrier(_commandBuffer,
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
