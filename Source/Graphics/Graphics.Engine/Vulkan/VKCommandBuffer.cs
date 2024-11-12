using System.Numerics;
using Graphics.Core.Helpers;
using Graphics.Engine.Enums;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using VkViewport = Silk.NET.Vulkan.Viewport;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKCommandBuffer : CommandBuffer
{
    private VKFrameBuffer? current;

    public VKCommandBuffer(Context context, VKCommandProcessor processor) : base(context)
    {
        Processor = processor;

        CommandPoolCreateInfo createInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = Processor.FamilyIndex,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };

        VkCommandPool commandPool;
        Context.Vk.CreateCommandPool(Context.Device, &createInfo, null, &commandPool).ThrowCode();

        CommandBufferAllocateInfo allocateInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        VkCommandBuffer commandBuffer;
        Context.Vk.AllocateCommandBuffers(Context.Device, &allocateInfo, &commandBuffer).ThrowCode();

        CommandPool = commandPool;
        CommandBuffer = commandBuffer;
    }

    public new VKContext Context => (VKContext)base.Context;

    public VKCommandProcessor Processor { get; }

    public VkCommandPool CommandPool { get; }

    public VkCommandBuffer CommandBuffer { get; }

    public override void Begin()
    {
        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        Context.Vk.BeginCommandBuffer(CommandBuffer, &beginInfo).ThrowCode();

        current = null;
    }

    public override void Reset()
    {
        Context.Vk.ResetCommandBuffer(CommandBuffer, CommandBufferResetFlags.None).ThrowCode();
    }

    public override void Commit()
    {
        Processor.CommitCommandBuffer(this);
    }

    public override void BeginRendering(FrameBuffer frameBuffer, ClearValue clearValue)
    {
        current?.TransitionToFinalLayout(CommandBuffer);

        current = frameBuffer.VK();

        current.TransitionToIntermedialLayout(CommandBuffer);

        Context.Vk.CmdBeginRendering(CommandBuffer, in current.RenderingInfo);

        if (clearValue.Options.HasFlag(ClearOptions.Color))
        {
            for (int i = 0; i < clearValue.ColorValues.Length; i++)
            {
                Vector4 color = clearValue.ColorValues[i];

                ClearAttachment clearAttachment = new()
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    ColorAttachment = (uint)i,
                    ClearValue = new()
                    {
                        Color = new()
                        {
                            Float32_0 = color.X,
                            Float32_1 = color.Y,
                            Float32_2 = color.Z,
                            Float32_3 = color.W
                        }
                    }
                };

                ClearRect clearRect = new()
                {
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                    Rect = new()
                    {
                        Offset = new()
                        {
                            X = 0,
                            Y = 0
                        },
                        Extent = new()
                        {
                            Width = current.Width,
                            Height = current.Height
                        }
                    }
                };

                Context.Vk.CmdClearAttachments(CommandBuffer, 1, &clearAttachment, 1, &clearRect);
            }
        }

        bool clearDepth = clearValue.Options.HasFlag(ClearOptions.Depth);
        bool clearStencil = clearValue.Options.HasFlag(ClearOptions.Stencil);

        if (current.DepthStencilTarget != null && (clearDepth || clearStencil))
        {
            ImageAspectFlags aspectMask = 0;

            if (clearDepth)
            {
                aspectMask |= ImageAspectFlags.DepthBit;
            }

            if (clearStencil)
            {
                aspectMask |= ImageAspectFlags.StencilBit;
            }

            ClearAttachment clearAttachment = new()
            {
                AspectMask = aspectMask,
                ClearValue = new()
                {
                    DepthStencil = new()
                    {
                        Depth = clearValue.Depth,
                        Stencil = clearValue.Stencil
                    }
                }
            };

            ClearRect clearRect = new()
            {
                BaseArrayLayer = 0,
                LayerCount = 1,
                Rect = new()
                {
                    Offset = new()
                    {
                        X = 0,
                        Y = 0
                    },
                    Extent = new()
                    {
                        Width = current.Width,
                        Height = current.Height
                    }
                }
            };

            Context.Vk.CmdClearAttachments(CommandBuffer, 1, &clearAttachment, 1, &clearRect);
        }
    }

    public override void EndRendering()
    {
        Context.Vk.CmdEndRendering(CommandBuffer);

        current?.TransitionToFinalLayout(CommandBuffer);
    }

    public override void SetViewports(Viewport[] viewports)
    {
        VkViewport[] vkViewports = viewports.Select(viewport => new VkViewport
        {
            X = viewport.X,
            Y = viewport.Y + viewport.Height,
            Width = viewport.Width,
            Height = -viewport.Height,
            MinDepth = viewport.MinDepth,
            MaxDepth = viewport.MaxDepth
        }).ToArray();

        Context.Vk.CmdSetViewport(CommandBuffer, 0, (uint)vkViewports.Length, vkViewports.AsPointer());
    }

    public override void SetScissorRectangles(Rectangle<int>[] scissors)
    {
        Rect2D[] vkScissors = scissors.Select(scissor => new Rect2D
        {
            Offset = new Offset2D
            {
                X = scissor.Origin.X,
                Y = scissor.Origin.Y
            },
            Extent = new Extent2D
            {
                Width = (uint)scissor.Size.X,
                Height = (uint)scissor.Size.Y
            }
        }).ToArray();

        Context.Vk.CmdSetScissor(CommandBuffer, 0, (uint)vkScissors.Length, vkScissors.AsPointer());
    }

    public override void SetFullViewports()
    {
    }

    public override void SetFullScissorRectangles()
    {
    }

    public override void SetPipeline(Pipeline pipeline)
    {
        throw new NotImplementedException();
    }

    public override void SetVertexBuffer(uint slot, Buffer buffer, uint offset = 0)
    {
        throw new NotImplementedException();
    }

    public override void SetIndexBuffer(Buffer buffer, IndexFormat format = IndexFormat.U16Bit, uint offset = 0)
    {
        throw new NotImplementedException();
    }

    public override void SetResourceSet(ResourceSet resourceSet, uint index = 0, uint[]? constantBufferOffsets = null)
    {
        throw new NotImplementedException();
    }

    public override void DrawInstanced(uint vertexCountPerInstance, uint instanceCount, uint startVertexLocation = 0, uint startInstanceLocation = 0)
    {
        throw new NotImplementedException();
    }

    public override void DrawInstancedIndirect(Buffer argBuffer, uint offset, uint drawCount, uint stride)
    {
        throw new NotImplementedException();
    }

    public override void DrawIndexed(uint indexCount, uint startIndexLocation = 0, uint baseVertexLocation = 0)
    {
        throw new NotImplementedException();
    }

    public override void DrawIndexedInstanced(uint indexCountPerInstance, uint instanceCount, uint startIndexLocation = 0, uint baseVertexLocation = 0, uint startInstanceLocation = 0)
    {
        throw new NotImplementedException();
    }

    public override void DrawIndexedInstancedIndirect(Buffer argBuffer, uint offset, uint drawCount, uint stride)
    {
        throw new NotImplementedException();
    }

    public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
    {
        throw new NotImplementedException();
    }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.CommandPool, CommandPool.Handle, name);
        Context.SetDebugName(ObjectType.CommandBuffer, (ulong)CommandBuffer.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyCommandPool(Context.Device, CommandPool, null);
    }

    protected override void ClearCache()
    {
    }

    protected override void EndInternal()
    {
        Context.Vk.EndCommandBuffer(CommandBuffer).ThrowCode();
    }
}
