using System.Runtime.CompilerServices;
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
            throw new InvalidOperationException("Command list is already recording");
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
        EndCurrentRenderPass();

        _currentFramebuffer = framebuffer;

        BeginCurrentRenderPass();
    }

    public void ClearColorTarget(uint index, RgbaFloat clearColor)
    {
        if (_currentFramebuffer == null)
        {
            throw new InvalidOperationException("No framebuffer set");
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
            throw new InvalidOperationException("No framebuffer set");
        }

        if (_currentFramebuffer.DepthAttachmentCount == 0)
        {
            throw new InvalidOperationException("Framebuffer does not have a depth attachment");
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

    public void End()
    {
        if (!_isRecording)
        {
            throw new InvalidOperationException("Command list is not recording");
        }

        EndCurrentRenderPass();

        Vk.EndCommandBuffer(_commandBuffer).ThrowCode();

        _currentFramebuffer = null;

        _isRecording = false;
    }

    protected override void Destroy()
    {
        Vk.FreeCommandBuffers(Device, _commandPool, 1, [_commandBuffer]);
    }

    private void BeginCurrentRenderPass()
    {
        if (_currentFramebuffer == null)
        {
            throw new InvalidOperationException("No framebuffer set");
        }

        ClearColorValue[] clearColorValues = new ClearColorValue[_currentFramebuffer.AttachmentCount];
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
    }

    private void EndCurrentRenderPass()
    {
        if (_currentFramebuffer == null)
        {
            return;
        }

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
    }
}
