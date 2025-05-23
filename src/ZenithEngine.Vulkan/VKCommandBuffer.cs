﻿using Silk.NET.Maths;
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

        ImageLayout oldLayout = vkTexture[region.Position.MipLevel,
                                          region.Position.ArrayLayer,
                                          region.Position.Face];

        vkTexture.TransitionLayout(CommandBuffer,
                                   region.Position.MipLevel,
                                   1,
                                   region.Position.ArrayLayer,
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
                                   region.Position.ArrayLayer,
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

        ImageLayout srcOldLayout = src[sourcePosition.MipLevel,
                                       sourcePosition.ArrayLayer,
                                       sourcePosition.Face];
        ImageLayout dstOldLayout = dst[destinationPosition.MipLevel,
                                       destinationPosition.ArrayLayer,
                                       destinationPosition.Face];

        src.TransitionLayout(CommandBuffer,
                             sourcePosition.MipLevel,
                             1,
                             sourcePosition.ArrayLayer,
                             1,
                             sourcePosition.Face,
                             1,
                             ImageLayout.TransferSrcOptimal);

        dst.TransitionLayout(CommandBuffer,
                             destinationPosition.MipLevel,
                             1,
                             destinationPosition.ArrayLayer,
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
                             sourcePosition.ArrayLayer,
                             1,
                             sourcePosition.Face,
                             1,
                             srcOldLayout);

        dst.TransitionLayout(CommandBuffer,
                             destinationPosition.MipLevel,
                             1,
                             destinationPosition.ArrayLayer,
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

        ImageLayout srcOldLayout = src[sourcePosition.MipLevel,
                                       sourcePosition.ArrayLayer,
                                       sourcePosition.Face];
        ImageLayout dstOldLayout = dst[destinationPosition.MipLevel,
                                       destinationPosition.ArrayLayer,
                                       destinationPosition.Face];

        src.TransitionLayout(CommandBuffer,
                             sourcePosition.MipLevel,
                             1,
                             sourcePosition.ArrayLayer,
                             1,
                             sourcePosition.Face,
                             1,
                             ImageLayout.TransferSrcOptimal);

        dst.TransitionLayout(CommandBuffer,
                             destinationPosition.MipLevel,
                             1,
                             destinationPosition.ArrayLayer,
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
                             sourcePosition.ArrayLayer,
                             1,
                             sourcePosition.Face,
                             1,
                             srcOldLayout);

        dst.TransitionLayout(CommandBuffer,
                             destinationPosition.MipLevel,
                             1,
                             destinationPosition.ArrayLayer,
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

        vkFrameBuffer.TransitionToIntermediateLayout(CommandBuffer);

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

        Viewport[] viewports = new Viewport[vkFrameBuffer.ColorAttachmentCount];
        Vector2D<int>[] scissorsByOffset = new Vector2D<int>[vkFrameBuffer.ColorAttachmentCount];
        Vector2D<uint>[] scissorsByExtent = new Vector2D<uint>[vkFrameBuffer.ColorAttachmentCount];

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
        activePipeline = pipeline;

        Context.Vk.CmdBindPipeline(CommandBuffer,
                                   PipelineBindPoint.RayTracingKhr,
                                   pipeline.VK().Pipeline);
    }
    #endregion

    #region Resource Binding Operations
    public override void PrepareResources(ResourceSet[] resourceSets)
    {
        foreach (ResourceSet resourceSet in resourceSets)
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
    }

    public override void SetVertexBuffer(uint slot, Buffer buffer, uint offset = 0)
    {
        ValidatePipeline<VKGraphicsPipeline>(out _);

        ulong longOffset = offset;

        Context.Vk.CmdBindVertexBuffers(CommandBuffer,
                                        slot,
                                        1,
                                        in buffer.VK().Buffer,
                                        in longOffset);
    }

    public override void SetVertexBuffers(Buffer[] buffers, uint[] offsets)
    {
        ValidatePipeline<VKGraphicsPipeline>(out _);

        Context.Vk.CmdBindVertexBuffers(CommandBuffer,
                                        0,
                                        (uint)buffers.Length,
                                        [.. buffers.Select(static item => item.VK().Buffer)],
                                        [.. offsets.Select(static item => (ulong)item)]);
    }

    public override void SetIndexBuffer(Buffer buffer,
                                        IndexFormat format = IndexFormat.UInt16,
                                        uint offset = 0)
    {
        ValidatePipeline<VKGraphicsPipeline>(out _);

        Context.Vk.CmdBindIndexBuffer(CommandBuffer,
                                      buffer.VK().Buffer,
                                      offset,
                                      VKFormats.GetIndexType(format));
    }

    public override void SetResourceSet(uint slot, ResourceSet resourceSet)
    {
        VKResourceSet vkResourceSet = resourceSet.VK();

        (PipelineBindPoint bindPoint, VkPipelineLayout layout) = activePipeline switch
        {
            VKGraphicsPipeline graphicsPipeline => (PipelineBindPoint.Graphics, graphicsPipeline.PipelineLayout),
            VKComputePipeline computePipeline => (PipelineBindPoint.Compute, computePipeline.PipelineLayout),
            VKRayTracingPipeline rayTracingPipeline => (PipelineBindPoint.RayTracingKhr, rayTracingPipeline.PipelineLayout),
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(activePipeline))
        };

        Context.Vk.CmdBindDescriptorSets(CommandBuffer,
                                         bindPoint,
                                         layout,
                                         slot,
                                         1,
                                         in vkResourceSet.Token.Set,
                                         0,
                                         null);
    }
    #endregion

    #region Drawing Operations
    public override void Draw(uint vertexCount,
                              uint instanceCount,
                              uint firstVertex = 0,
                              uint firstInstance = 0)
    {
        ValidatePipeline<VKGraphicsPipeline>(out _);

        Context.Vk.CmdDraw(CommandBuffer,
                           vertexCount,
                           instanceCount,
                           firstVertex,
                           firstInstance);
    }

    public override void DrawIndirect(Buffer argBuffer,
                                      uint offset,
                                      uint drawCount)
    {
        ValidatePipeline<VKGraphicsPipeline>(out _);

        Context.Vk.CmdDrawIndirect(CommandBuffer,
                                   argBuffer.VK().Buffer,
                                   offset,
                                   drawCount,
                                   (uint)sizeof(IndirectDrawArgs));
    }

    public override void DrawIndexed(uint indexCount,
                                     uint instanceCount,
                                     uint firstIndex = 0,
                                     int vertexOffset = 0,
                                     uint firstInstance = 0)
    {
        ValidatePipeline<VKGraphicsPipeline>(out _);

        Context.Vk.CmdDrawIndexed(CommandBuffer,
                                  indexCount,
                                  instanceCount,
                                  firstIndex,
                                  vertexOffset,
                                  firstInstance);
    }

    public override void DrawIndexedIndirect(Buffer argBuffer,
                                             uint offset,
                                             uint drawCount)
    {
        ValidatePipeline<VKGraphicsPipeline>(out _);

        Context.Vk.CmdDrawIndexedIndirect(CommandBuffer,
                                          argBuffer.VK().Buffer,
                                          offset,
                                          drawCount,
                                          (uint)sizeof(IndirectDrawIndexedArgs));
    }
    #endregion

    #region Compute Operations
    public override void Dispatch(uint groupCountX,
                                  uint groupCountY,
                                  uint groupCountZ)
    {
        ValidatePipeline<VKComputePipeline>(out _);

        Context.Vk.CmdDispatch(CommandBuffer,
                               groupCountX,
                               groupCountY,
                               groupCountZ);
    }

    public override void DispatchIndirect(Buffer argBuffer, uint offset)
    {
        ValidatePipeline<VKComputePipeline>(out _);

        Context.Vk.CmdDispatchIndirect(CommandBuffer,
                                       argBuffer.VK().Buffer,
                                       offset);
    }
    #endregion

    #region Ray Tracing Operations
    public override void DispatchRays(uint width, uint height, uint depth)
    {
        ValidatePipeline(out VKRayTracingPipeline vkPipeline);

        StridedDeviceAddressRegionKHR rayGenRegion = vkPipeline.ShaderTable.RayGenRegion;
        StridedDeviceAddressRegionKHR missRegion = vkPipeline.ShaderTable.MissRegion;
        StridedDeviceAddressRegionKHR hitGroupRegion = vkPipeline.ShaderTable.HitGroupRegion;
        StridedDeviceAddressRegionKHR callableRegion = new();

        Context.KhrRayTracingPipeline!.CmdTraceRays(CommandBuffer,
                                                    &rayGenRegion,
                                                    &missRegion,
                                                    &hitGroupRegion,
                                                    &callableRegion,
                                                    width,
                                                    height,
                                                    depth);
    }
    #endregion

    #region Debugging
    public override void BeginDebugEvent(string label)
    {
        DebugUtilsLabelEXT labelInfo = new()
        {
            SType = StructureType.DebugUtilsLabelExt,
            PLabelName = Allocator.AllocUTF8(label)
        };

        Context.ExtDebugUtils!.CmdBeginDebugUtilsLabel(CommandBuffer, &labelInfo);
    }

    public override void EndDebugEvent()
    {
        Context.ExtDebugUtils!.CmdEndDebugUtilsLabel(CommandBuffer);
    }

    public override void InsertDebugMarker(string label)
    {
        DebugUtilsLabelEXT labelInfo = new()
        {
            SType = StructureType.DebugUtilsLabelExt,
            PLabelName = Allocator.AllocUTF8(label)
        };

        Context.ExtDebugUtils!.CmdInsertDebugUtilsLabel(CommandBuffer, &labelInfo);
    }
    #endregion

    protected override void SetName(string name)
    {
        DebugUtilsObjectNameInfoEXT nameInfo = new()
        {
            SType = StructureType.DebugUtilsObjectNameInfoExt,
            ObjectType = ObjectType.CommandBuffer,
            ObjectHandle = (ulong)CommandBuffer.Handle,
            PObjectName = Allocator.AllocUTF8(name)
        };

        Context.ExtDebugUtils!.SetDebugUtilsObjectName(Context.Device, &nameInfo).ThrowIfError();
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyCommandPool(Context.Device, CommandPool, null);

        base.Destroy();
    }

    private void ValidatePipeline<T>(out T pipeline) where T : Pipeline
    {
        if (activePipeline is not T castedPipeline)
        {
            throw new ZenithEngineException(ExceptionHelpers.NotSupported(activePipeline));
        }

        pipeline = castedPipeline;
    }
}
