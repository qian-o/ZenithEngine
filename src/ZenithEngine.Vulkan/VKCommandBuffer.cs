using Silk.NET.Maths;
using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKCommandBuffer : CommandBuffer
{
    public VkCommandPool CommandPool;
    public VkCommandBuffer CommandBuffer;

    private FrameBuffer? activeFrameBuffer;
    private Pipeline? activePipeline;

    public VKCommandBuffer(GraphicsContext context,
                           CommandProcessor processor) : base(context, processor)
    {
        CommandPoolCreateInfo createInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = Context.FindQueueFamilyIndex(ProcessorType),
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };

        Context.Vk.CreateCommandPool(Context.Device, &createInfo, null, out CommandPool).ThrowIfError();

        CommandBufferAllocateInfo allocateInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = CommandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        Context.Vk.AllocateCommandBuffers(Context.Device, &allocateInfo, out CommandBuffer).ThrowIfError();
    }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    #region Command Buffer Management
    public override void Begin()
    {
        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        Context.Vk.BeginCommandBuffer(CommandBuffer, &beginInfo).ThrowIfError();
    }

    public override void End()
    {
        Context.Vk.EndCommandBuffer(CommandBuffer).ThrowIfError();

        activeFrameBuffer = null;
        activePipeline = null;
    }

    public override void Reset()
    {
        Context.Vk.ResetCommandBuffer(CommandBuffer, CommandBufferResetFlags.None).ThrowIfError();

        base.Reset();
    }
    #endregion

    #region Buffer Operations
    public override void CopyBuffer(Buffer source,
                                    Buffer destination,
                                    uint sizeInBytes,
                                    uint sourceOffsetInBytes = 0,
                                    uint destinationOffsetInBytes = 0)
    {
        VKBuffer src = source.VK();
        VKBuffer dst = destination.VK();

        BufferCopy bufferCopy = new()
        {
            Size = sizeInBytes,
            SrcOffset = sourceOffsetInBytes,
            DstOffset = destinationOffsetInBytes
        };

        Context.Vk.CmdCopyBuffer(CommandBuffer, src.Buffer, dst.Buffer, 1, &bufferCopy);

        MemoryBarrier barrier = new()
        {
            SType = StructureType.MemoryBarrier,
            SrcAccessMask = AccessFlags.MemoryWriteBit,
            DstAccessMask = AccessFlags.MemoryReadBit
        };

        PipelineStageFlags dstStageMask = ProcessorType switch
        {
            CommandProcessorType.Graphics => PipelineStageFlags.AllGraphicsBit,
            CommandProcessorType.Compute => PipelineStageFlags.ComputeShaderBit,
            _ => PipelineStageFlags.AllCommandsBit
        };

        Context.Vk.CmdPipelineBarrier(CommandBuffer,
                                      PipelineStageFlags.TransferBit,
                                      dstStageMask,
                                      DependencyFlags.None,
                                      1,
                                      &barrier,
                                      0,
                                      null,
                                      0,
                                      null);
    }
    #endregion

    #region Texture Operations
    public override void UpdateTexture(Texture texture,
                                       nint source,
                                       uint sourceSizeInBytes,
                                       TextureRegion region)
    {
        Buffer temporary = BufferAllocator.Buffer(sourceSizeInBytes);

        Context.UpdateBuffer(temporary, source, sourceSizeInBytes);

        VKTexture vkTexture = texture.VK();

        ImageLayout oldLayout = vkTexture[region.Position.MipLevel, region.Position.Face];

        vkTexture.TransitionLayout(CommandBuffer,
                                   region.Position.MipLevel,
                                   1,
                                   region.Position.Face,
                                   1,
                                   ImageLayout.TransferDstOptimal);

        BufferImageCopy bufferImageCopy = new()
        {
            ImageSubresource = new()
            {
                AspectMask = VKFormats.GetImageAspectFlags(vkTexture.Desc.Usage),
                MipLevel = region.Position.MipLevel,
                BaseArrayLayer = (uint)region.Position.Face,
                LayerCount = 1
            },
            ImageOffset = new()
            {
                X = (int)region.Position.X,
                Y = (int)region.Position.Y,
                Z = (int)region.Position.Z
            },
            ImageExtent = new()
            {
                Width = region.Width,
                Height = region.Height,
                Depth = region.Depth
            }
        };

        Context.Vk.CmdCopyBufferToImage(CommandBuffer,
                                        temporary.VK().Buffer,
                                        vkTexture.Image,
                                        ImageLayout.TransferDstOptimal,
                                        1,
                                        &bufferImageCopy);

        vkTexture.TransitionLayout(CommandBuffer,
                                   region.Position.MipLevel,
                                   1,
                                   region.Position.Face,
                                   1,
                                   oldLayout);
    }

    public override void CopyTexture(Texture source,
                                     TexturePosition sourcePosition,
                                     Texture destination,
                                     TexturePosition destinationPosition,
                                     uint width,
                                     uint height,
                                     uint depth)
    {
        VKTexture src = source.VK();
        VKTexture dst = destination.VK();

        ImageLayout srcOldLayout = src[sourcePosition.MipLevel, sourcePosition.Face];
        ImageLayout dstOldLayout = dst[destinationPosition.MipLevel, destinationPosition.Face];

        src.TransitionLayout(CommandBuffer,
                             sourcePosition.MipLevel,
                             1,
                             sourcePosition.Face,
                             1,
                             ImageLayout.TransferSrcOptimal);

        dst.TransitionLayout(CommandBuffer,
                             destinationPosition.MipLevel,
                             1,
                             destinationPosition.Face,
                             1,
                             ImageLayout.TransferDstOptimal);

        ImageCopy imageCopy = new()
        {
            SrcSubresource = new()
            {
                AspectMask = VKFormats.GetImageAspectFlags(src.Desc.Usage),
                MipLevel = sourcePosition.MipLevel,
                BaseArrayLayer = (uint)sourcePosition.Face,
                LayerCount = 1
            },
            SrcOffset = new()
            {
                X = (int)sourcePosition.X,
                Y = (int)sourcePosition.Y,
                Z = (int)sourcePosition.Z
            },
            DstSubresource = new()
            {
                AspectMask = VKFormats.GetImageAspectFlags(dst.Desc.Usage),
                MipLevel = destinationPosition.MipLevel,
                BaseArrayLayer = (uint)destinationPosition.Face,
                LayerCount = 1
            },
            DstOffset = new()
            {
                X = (int)destinationPosition.X,
                Y = (int)destinationPosition.Y,
                Z = (int)destinationPosition.Z
            },
            Extent = new()
            {
                Width = width,
                Height = height,
                Depth = depth
            }
        };

        Context.Vk.CmdCopyImage(CommandBuffer,
                                src.Image,
                                ImageLayout.TransferSrcOptimal,
                                dst.Image,
                                ImageLayout.TransferDstOptimal,
                                1,
                                &imageCopy);

        src.TransitionLayout(CommandBuffer,
                             sourcePosition.MipLevel,
                             1,
                             sourcePosition.Face,
                             1,
                             srcOldLayout);

        dst.TransitionLayout(CommandBuffer,
                             destinationPosition.MipLevel,
                             1,
                             destinationPosition.Face,
                             1,
                             dstOldLayout);
    }

    public override void ResolveTexture(Texture source,
                                        TexturePosition sourcePosition,
                                        Texture destination,
                                        TexturePosition destinationPosition)
    {
        Utils.GetMipDimensions(sourcePosition.MipLevel,
                               source.Desc.Width,
                               source.Desc.Height,
                               source.Desc.Depth,
                               out uint srcWidth,
                               out uint srcHeight,
                               out uint srcDepth);

        srcWidth -= sourcePosition.X;
        srcHeight -= sourcePosition.Y;
        srcDepth -= sourcePosition.Z;

        Utils.GetMipDimensions(destinationPosition.MipLevel,
                               destination.Desc.Width,
                               destination.Desc.Height,
                               destination.Desc.Depth,
                               out uint dstWidth,
                               out uint dstHeight,
                               out uint dstDepth);

        dstWidth -= destinationPosition.X;
        dstHeight -= destinationPosition.Y;
        dstDepth -= destinationPosition.Z;

        uint width = Math.Min(srcWidth, dstWidth);
        uint height = Math.Min(srcHeight, dstHeight);
        uint depth = Math.Min(srcDepth, dstDepth);

        VKTexture src = source.VK();
        VKTexture dst = destination.VK();

        ImageLayout srcOldLayout = src[sourcePosition.MipLevel, sourcePosition.Face];
        ImageLayout dstOldLayout = dst[destinationPosition.MipLevel, destinationPosition.Face];

        src.TransitionLayout(CommandBuffer,
                             sourcePosition.MipLevel,
                             1,
                             sourcePosition.Face,
                             1,
                             ImageLayout.TransferSrcOptimal);

        dst.TransitionLayout(CommandBuffer,
                             destinationPosition.MipLevel,
                             1,
                             destinationPosition.Face,
                             1,
                             ImageLayout.TransferDstOptimal);

        ImageResolve imageResolve = new()
        {
            SrcSubresource = new()
            {
                AspectMask = VKFormats.GetImageAspectFlags(src.Desc.Usage),
                MipLevel = sourcePosition.MipLevel,
                BaseArrayLayer = (uint)sourcePosition.Face,
                LayerCount = 1
            },
            SrcOffset = new()
            {
                X = (int)sourcePosition.X,
                Y = (int)sourcePosition.Y,
                Z = (int)sourcePosition.Z
            },
            DstSubresource = new()
            {
                AspectMask = VKFormats.GetImageAspectFlags(dst.Desc.Usage),
                MipLevel = destinationPosition.MipLevel,
                BaseArrayLayer = (uint)destinationPosition.Face,
                LayerCount = 1
            },
            DstOffset = new()
            {
                X = (int)destinationPosition.X,
                Y = (int)destinationPosition.Y,
                Z = (int)destinationPosition.Z
            },
            Extent = new()
            {
                Width = width,
                Height = height,
                Depth = depth
            }
        };

        Context.Vk.CmdResolveImage(CommandBuffer,
                                   src.Image,
                                   ImageLayout.TransferSrcOptimal,
                                   dst.Image,
                                   ImageLayout.TransferDstOptimal,
                                   1,
                                   &imageResolve);

        src.TransitionLayout(CommandBuffer,
                             sourcePosition.MipLevel,
                             1,
                             sourcePosition.Face,
                             1,
                             srcOldLayout);

        dst.TransitionLayout(CommandBuffer,
                             destinationPosition.MipLevel,
                             1,
                             destinationPosition.Face,
                             1,
                             dstOldLayout);
    }
    #endregion

    #region Acceleration Structure Operations
    public override BottomLevelAS BuildAccelerationStructure(ref readonly BottomLevelASDesc desc)
    {
        return new VKBottomLevelAS(Context, CommandBuffer, in desc);
    }

    public override TopLevelAS BuildAccelerationStructure(ref readonly TopLevelASDesc desc)
    {
        return new VKTopLevelAS(Context, CommandBuffer, in desc);
    }

    public override void UpdateAccelerationStructure(ref TopLevelAS tlas, ref readonly TopLevelASDesc newDesc)
    {
        tlas.VK().UpdateAccelerationStructure(CommandBuffer, in newDesc);
    }
    #endregion

    #region Rendering Operations
    public override void BeginRendering(FrameBuffer frameBuffer, ClearValue clearValue)
    {
        EndRendering();

        activeFrameBuffer = frameBuffer;

        VKFrameBuffer vkFrameBuffer = frameBuffer.VK();

        vkFrameBuffer.TransitionToIntermedialLayout(CommandBuffer);

        Context.Vk.CmdBeginRendering(CommandBuffer, in vkFrameBuffer.RenderingInfo);

        bool clearColor = clearValue.Options.HasFlag(ClearOptions.Color);
        bool clearDepth = clearValue.Options.HasFlag(ClearOptions.Depth);
        bool clearStencil = clearValue.Options.HasFlag(ClearOptions.Stencil);

        if (clearColor)
        {
            for (int i = 0; i < clearValue.ColorValues.Length; i++)
            {
                Vector4D<float> color = clearValue.ColorValues[i];

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
                        Extent = new()
                        {
                            Width = vkFrameBuffer.Width,
                            Height = vkFrameBuffer.Height
                        }
                    }
                };

                Context.Vk.CmdClearAttachments(CommandBuffer, 1, &clearAttachment, 1, &clearRect);
            }
        }

        if (clearDepth || clearStencil)
        {
            ClearAttachment clearAttachment = new()
            {
                AspectMask = (clearDepth ? ImageAspectFlags.DepthBit : 0)
                             | (clearStencil ? ImageAspectFlags.StencilBit : 0),
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
                    Extent = new()
                    {
                        Width = vkFrameBuffer.Width,
                        Height = vkFrameBuffer.Height
                    }
                }
            };

            Context.Vk.CmdClearAttachments(CommandBuffer, 1, &clearAttachment, 1, &clearRect);
        }

        Viewport[] viewports = new Viewport[vkFrameBuffer.ColorViews.Length];
        Vector2D<int>[] scissorsByOffset = new Vector2D<int>[vkFrameBuffer.ColorViews.Length];
        Vector2D<uint>[] scissorsByExtent = new Vector2D<uint>[vkFrameBuffer.ColorViews.Length];

        Array.Fill(viewports, new(0, 0, vkFrameBuffer.Width, vkFrameBuffer.Height));
        Array.Fill(scissorsByOffset, new(0, 0));
        Array.Fill(scissorsByExtent, new(vkFrameBuffer.Width, vkFrameBuffer.Height));

        SetViewports(viewports);
        SetScissorRectangles(scissorsByOffset, scissorsByExtent);
    }

    public override void EndRendering()
    {
        if (activeFrameBuffer is null)
        {
            return;
        }

        Context.Vk.CmdEndRendering(CommandBuffer);

        activeFrameBuffer.VK().TransitionToFinalLayout(CommandBuffer);

        activeFrameBuffer = null;
    }

    public override void SetViewports(Viewport[] viewports)
    {
        Context.Vk.CmdSetViewport(CommandBuffer, 0, (uint)viewports.Length, [.. viewports.Select(static item => new VkViewport
        {
            X = item.X,
            Y = item.Y + item.Height,
            Width = item.Width,
            Height = -item.Height,
            MinDepth = item.MinDepth,
            MaxDepth = item.MaxDepth
        })]);
    }

    public override void SetScissorRectangles(Vector2D<int>[] offsets, Vector2D<uint>[] extents)
    {
        uint count = (uint)Math.Min(offsets.Length, extents.Length);

        Rect2D[] scs = new Rect2D[count];

        for (uint i = 0; i < count; i++)
        {
            scs[i] = new()
            {
                Offset = new()
                {
                    X = offsets[i].X,
                    Y = offsets[i].Y
                },
                Extent = new()
                {
                    Width = extents[i].X,
                    Height = extents[i].Y
                }
            };
        }

        Context.Vk.CmdSetScissor(CommandBuffer, 0, (uint)scs.Length, scs);
    }
    #endregion

    #region Pipeline Operations
    public override void SetGraphicsPipeline(GraphicsPipeline pipeline)
    {
        activePipeline = pipeline;

        Context.Vk.CmdBindPipeline(CommandBuffer,
                                   PipelineBindPoint.Graphics,
                                   pipeline.VK().Pipeline);
    }

    public override void SetComputePipeline(ComputePipeline pipeline)
    {
        activePipeline = pipeline;

        Context.Vk.CmdBindPipeline(CommandBuffer,
                                   PipelineBindPoint.Compute,
                                   pipeline.VK().Pipeline);
    }

    public override void SetRayTracingPipeline(RayTracingPipeline pipeline)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Resource Binding Operations
    public override void SetVertexBuffer(uint slot, Buffer buffer, uint offset = 0)
    {
        ulong longOffset = offset;

        Context.Vk.CmdBindVertexBuffers(CommandBuffer,
                                        slot,
                                        1,
                                        in buffer.VK().Buffer,
                                        in longOffset);
    }

    public override void SetVertexBuffers(Buffer[] buffers, uint[] offsets)
    {
        Context.Vk.CmdBindVertexBuffers(CommandBuffer,
                                        0,
                                        (uint)buffers.Length,
                                        [.. buffers.Select(static item => item.VK().Buffer)],
                                        [.. offsets.Select(static item => (ulong)item)]);
    }

    public override void SetIndexBuffer(Buffer buffer,
                                        IndexFormat format = IndexFormat.U16Bit,
                                        uint offset = 0)
    {
        Context.Vk.CmdBindIndexBuffer(CommandBuffer,
                                      buffer.VK().Buffer,
                                      offset,
                                      VKFormats.GetIndexType(format));
    }

    public override void PrepareResources(ResourceSet resourceSet)
    {
        VKResourceSet vkResourceSet = resourceSet.VK();

        foreach (VKTexture texture in vkResourceSet.SrvTextures)
        {
            texture.TransitionLayout(CommandBuffer, ImageLayout.ShaderReadOnlyOptimal);
        }

        foreach (VKTexture texture in vkResourceSet.UavTextures)
        {
            texture.TransitionLayout(CommandBuffer, ImageLayout.General);
        }
    }

    public override void SetResourceSet(uint slot,
                                        ResourceSet resourceSet,
                                        uint[]? constantBufferOffsets = null)
    {
        VKResourceSet vkResourceSet = resourceSet.VK();

        uint[] offsets = new uint[vkResourceSet.DynamicConstantBufferCount];
        if (constantBufferOffsets is not null)
        {
            for (uint i = 0; i < vkResourceSet.DynamicConstantBufferCount; i++)
            {
                offsets[i] = constantBufferOffsets[i];
            }
        }

        (PipelineBindPoint bindPoint, VkPipelineLayout layout) = activePipeline switch
        {
            VKGraphicsPipeline graphicsPipeline => (PipelineBindPoint.Graphics, graphicsPipeline.PipelineLayout),
            VKComputePipeline computePipeline => (PipelineBindPoint.Compute, computePipeline.PipelineLayout),
            _ => throw new InvalidOperationException()
        };

        if (offsets.Length > 0)
        {
            Context.Vk.CmdBindDescriptorSets(CommandBuffer,
                                             bindPoint,
                                             layout,
                                             slot,
                                             1,
                                             in vkResourceSet.Token.Set,
                                             (uint)offsets.Length,
                                             in offsets[0]);
        }
        else
        {
            Context.Vk.CmdBindDescriptorSets(CommandBuffer,
                                             bindPoint,
                                             layout,
                                             slot,
                                             1,
                                             in vkResourceSet.Token.Set,
                                             0,
                                             null);
        }
    }
    #endregion

    #region Drawing Operations
    public override void DrawInstanced(uint vertexCountPerInstance,
                                       uint instanceCount,
                                       uint startVertexLocation = 0,
                                       uint startInstanceLocation = 0)
    {
        Context.Vk.CmdDraw(CommandBuffer,
                           vertexCountPerInstance,
                           instanceCount,
                           startVertexLocation,
                           startInstanceLocation);
    }

    public override void DrawInstancedIndirect(Buffer argBuffer,
                                               uint offset,
                                               uint drawCount,
                                               uint stride)
    {
        Context.Vk.CmdDrawIndirect(CommandBuffer,
                                   argBuffer.VK().Buffer,
                                   offset,
                                   drawCount,
                                   stride);
    }

    public override void DrawIndexed(uint indexCount,
                                     uint startIndexLocation = 0,
                                     uint baseVertexLocation = 0)
    {
        Context.Vk.CmdDrawIndexed(CommandBuffer,
                                  indexCount,
                                  1,
                                  startIndexLocation,
                                  (int)baseVertexLocation,
                                  0);
    }

    public override void DrawIndexedInstanced(uint indexCountPerInstance,
                                              uint instanceCount,
                                              uint startIndexLocation = 0,
                                              uint baseVertexLocation = 0,
                                              uint startInstanceLocation = 0)
    {
        Context.Vk.CmdDrawIndexed(CommandBuffer,
                                  indexCountPerInstance,
                                  instanceCount,
                                  startIndexLocation,
                                  (int)baseVertexLocation,
                                  startInstanceLocation);
    }

    public override void DrawIndexedInstancedIndirect(Buffer argBuffer,
                                                      uint offset,
                                                      uint drawCount,
                                                      uint stride)
    {
        Context.Vk.CmdDrawIndexedIndirect(CommandBuffer,
                                          argBuffer.VK().Buffer,
                                          offset,
                                          drawCount,
                                          stride);
    }
    #endregion

    #region Compute Operations
    public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
    {
        Context.Vk.CmdDispatch(CommandBuffer, groupCountX, groupCountY, groupCountZ);
    }
    #endregion

    #region Ray Tracing Operations
    public override void DispatchRays(uint width, uint height, uint depth)
    {
        throw new NotImplementedException();
    }
    #endregion

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.CommandBuffer, (ulong)CommandBuffer.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyCommandPool(Context.Device, CommandPool, null);

        base.Destroy();
    }
}
