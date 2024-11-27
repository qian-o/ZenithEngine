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
            QueueFamilyIndex = Context.FindQueueFamilyIndex(processor.Type),
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

        Context.Vk.CmdPipelineBarrier(CommandBuffer,
                                      PipelineStageFlags.TransferBit,
                                      PipelineStageFlags.AllGraphicsBit,
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

        ImageLayout oldLayout = vkTexture[region.MipLevel, region.Face];

        vkTexture.TransitionLayout(CommandBuffer,
                                   region.MipLevel,
                                   1,
                                   region.Face,
                                   1,
                                   ImageLayout.TransferDstOptimal);

        BufferImageCopy bufferImageCopy = new()
        {
            ImageSubresource = new()
            {
                AspectMask = VKFormats.GetImageAspectFlags(vkTexture.Desc.Usage),
                MipLevel = region.MipLevel,
                BaseArrayLayer = (uint)region.Face,
                LayerCount = 1
            },
            ImageOffset = new()
            {
                X = (int)region.X,
                Y = (int)region.Y,
                Z = (int)region.Z
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
                                   region.MipLevel,
                                   1,
                                   region.Face,
                                   1,
                                   oldLayout);
    }

    public override void CopyTexture(Texture source,
                                     TextureRegion sourceRegion,
                                     Texture destination,
                                     TextureRegion destinationRegion)
    {
        VKTexture src = source.VK();
        VKTexture dst = destination.VK();

        ImageLayout srcOldLayout = src[sourceRegion.MipLevel, sourceRegion.Face];
        ImageLayout dstOldLayout = dst[destinationRegion.MipLevel, destinationRegion.Face];

        src.TransitionLayout(CommandBuffer,
                             sourceRegion.MipLevel,
                             1,
                             sourceRegion.Face,
                             1,
                             ImageLayout.TransferSrcOptimal);

        dst.TransitionLayout(CommandBuffer,
                             destinationRegion.MipLevel,
                             1,
                             destinationRegion.Face,
                             1,
                             ImageLayout.TransferDstOptimal);

        if (sourceRegion.SizeEquals(destinationRegion))
        {
            ImageCopy imageCopy = new()
            {
                SrcSubresource = new()
                {
                    AspectMask = VKFormats.GetImageAspectFlags(src.Desc.Usage),
                    MipLevel = sourceRegion.MipLevel,
                    BaseArrayLayer = (uint)sourceRegion.Face,
                    LayerCount = 1
                },
                SrcOffset = new()
                {
                    X = (int)sourceRegion.X,
                    Y = (int)sourceRegion.Y,
                    Z = (int)sourceRegion.Z
                },
                DstSubresource = new()
                {
                    AspectMask = VKFormats.GetImageAspectFlags(dst.Desc.Usage),
                    MipLevel = destinationRegion.MipLevel,
                    BaseArrayLayer = (uint)destinationRegion.Face,
                    LayerCount = 1
                },
                DstOffset = new()
                {
                    X = (int)destinationRegion.X,
                    Y = (int)destinationRegion.Y,
                    Z = (int)destinationRegion.Z
                },
                Extent = new()
                {
                    Width = sourceRegion.Width,
                    Height = sourceRegion.Height,
                    Depth = sourceRegion.Depth
                }
            };

            Context.Vk.CmdCopyImage(CommandBuffer,
                                    src.Image,
                                    ImageLayout.TransferSrcOptimal,
                                    dst.Image,
                                    ImageLayout.TransferDstOptimal,
                                    1,
                                    &imageCopy);
        }
        else
        {
            ImageBlit imageBlit = new()
            {
                SrcSubresource = new()
                {
                    AspectMask = VKFormats.GetImageAspectFlags(src.Desc.Usage),
                    MipLevel = sourceRegion.MipLevel,
                    BaseArrayLayer = (uint)sourceRegion.Face,
                    LayerCount = 1
                },
                SrcOffsets = new()
                {
                    Element0 = new()
                    {
                        X = (int)sourceRegion.X,
                        Y = (int)sourceRegion.Y,
                        Z = (int)sourceRegion.Z
                    },
                    Element1 = new()
                    {
                        X = (int)sourceRegion.X + (int)sourceRegion.Width,
                        Y = (int)sourceRegion.Y + (int)sourceRegion.Height,
                        Z = (int)sourceRegion.Z + (int)sourceRegion.Depth
                    }
                },
                DstSubresource = new()
                {
                    AspectMask = VKFormats.GetImageAspectFlags(dst.Desc.Usage),
                    MipLevel = destinationRegion.MipLevel,
                    BaseArrayLayer = (uint)destinationRegion.Face,
                    LayerCount = 1
                },
                DstOffsets = new()
                {
                    Element0 = new()
                    {
                        X = (int)destinationRegion.X,
                        Y = (int)destinationRegion.Y,
                        Z = (int)destinationRegion.Z
                    },
                    Element1 = new()
                    {
                        X = (int)destinationRegion.X + (int)destinationRegion.Width,
                        Y = (int)destinationRegion.Y + (int)destinationRegion.Height,
                        Z = (int)destinationRegion.Z + (int)destinationRegion.Depth
                    }
                }
            };

            Context.Vk.CmdBlitImage(CommandBuffer,
                                    src.Image,
                                    ImageLayout.TransferSrcOptimal,
                                    dst.Image,
                                    ImageLayout.TransferDstOptimal,
                                    1,
                                    &imageBlit,
                                    Filter.Linear);
        }

        src.TransitionLayout(CommandBuffer,
                             sourceRegion.MipLevel,
                             1,
                             sourceRegion.Face,
                             1,
                             srcOldLayout);

        dst.TransitionLayout(CommandBuffer,
                             destinationRegion.MipLevel,
                             1,
                             destinationRegion.Face,
                             1,
                             dstOldLayout);
    }

    public override void GenerateMipmaps(Texture texture)
    {
        uint width = texture.Desc.Width;
        uint height = texture.Desc.Height;
        uint depth = texture.Desc.Depth;
        uint mipLevels = texture.Desc.MipLevels;
        uint arrayLayers = VKHelpers.GetArrayLayers(texture.Desc);

        VKTexture vkTexture = texture.VK();

        for (uint i = 1; i < mipLevels; i++)
        {
            uint srcMipLevel = i - 1;
            uint dstMipLevel = i;

            ImageLayout srcOldLayout = vkTexture[srcMipLevel, CubeMapFace.PositiveX];
            ImageLayout dstOldLayout = vkTexture[dstMipLevel, CubeMapFace.PositiveX];

            vkTexture.TransitionLayout(CommandBuffer,
                                       srcMipLevel,
                                       1,
                                       CubeMapFace.PositiveX,
                                       arrayLayers,
                                       ImageLayout.TransferSrcOptimal);

            vkTexture.TransitionLayout(CommandBuffer,
                                       dstMipLevel,
                                       1,
                                       CubeMapFace.PositiveX,
                                       arrayLayers,
                                       ImageLayout.TransferDstOptimal);

            uint mipWidth = Math.Max(1, width >> (int)i);
            uint mipHeight = Math.Max(1, height >> (int)i);
            uint mipDepth = Math.Max(1, depth >> (int)i);

            ImageBlit imageBlit = new()
            {
                SrcSubresource = new()
                {
                    AspectMask = VKFormats.GetImageAspectFlags(vkTexture.Desc.Usage),
                    MipLevel = srcMipLevel,
                    BaseArrayLayer = 0,
                    LayerCount = arrayLayers
                },
                SrcOffsets = new()
                {
                    Element1 = new()
                    {
                        X = (int)width,
                        Y = (int)height,
                        Z = (int)depth
                    }
                },
                DstSubresource = new()
                {
                    AspectMask = VKFormats.GetImageAspectFlags(vkTexture.Desc.Usage),
                    MipLevel = dstMipLevel,
                    BaseArrayLayer = 0,
                    LayerCount = arrayLayers
                },
                DstOffsets = new()
                {
                    Element1 = new()
                    {
                        X = (int)mipWidth,
                        Y = (int)mipHeight,
                        Z = (int)mipDepth
                    }
                }
            };

            Context.Vk.CmdBlitImage(CommandBuffer,
                                    vkTexture.Image,
                                    ImageLayout.TransferSrcOptimal,
                                    vkTexture.Image,
                                    ImageLayout.TransferDstOptimal,
                                    1,
                                    &imageBlit,
                                    Filter.Linear);

            vkTexture.TransitionLayout(CommandBuffer,
                                       srcMipLevel,
                                       1,
                                       CubeMapFace.PositiveX,
                                       arrayLayers,
                                       srcOldLayout);

            vkTexture.TransitionLayout(CommandBuffer,
                                       dstMipLevel,
                                       1,
                                       CubeMapFace.PositiveX,
                                       arrayLayers,
                                       dstOldLayout);

            width = mipWidth;
            height = mipHeight;
            depth = mipDepth;
        }
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

    public override void TransitionTexture(Texture texture, TextureUsage usage)
    {
        texture.VK().TransitionLayout(CommandBuffer, VKFormats.GetImageLayout(usage));
    }
    #endregion

    #region Acceleration Structure Operations
    public override BottomLevelAS BuildAccelerationStructure(ref readonly BottomLevelASDesc desc)
    {
        throw new NotImplementedException();
    }

    public override TopLevelAS BuildAccelerationStructure(ref readonly TopLevelASDesc desc)
    {
        throw new NotImplementedException();
    }

    public override void UpdateAccelerationStructure(ref TopLevelAS tlas, ref readonly TopLevelASDesc newDesc)
    {
        throw new NotImplementedException();
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
            for (uint i = 0; i < clearValue.ColorValues.Length; i++)
            {
                Vector4D<float> color = clearValue.ColorValues[i];

                ClearAttachment clearAttachment = new()
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    ColorAttachment = i,
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

    public override void SetViewport(uint slot, Viewport viewport)
    {
        VkViewport vp = new()
        {
            X = viewport.X,
            Y = viewport.Y + viewport.Height,
            Width = viewport.Width,
            Height = -viewport.Height,
            MinDepth = viewport.MinDepth,
            MaxDepth = viewport.MaxDepth
        };

        Context.Vk.CmdSetViewport(CommandBuffer, slot, 1, &vp);
    }

    public override void SetViewports(Viewport[] viewports)
    {
        VkViewport[] vps = viewports.Select(static item => new VkViewport
        {
            X = item.X,
            Y = item.Y + item.Height,
            Width = item.Width,
            Height = -item.Height,
            MinDepth = item.MinDepth,
            MaxDepth = item.MaxDepth
        }).ToArray();

        Context.Vk.CmdSetViewport(CommandBuffer, 0, (uint)vps.Length, vps);
    }

    public override void SetScissorRectangle(uint slot, Rectangle<int> scissor)
    {
        Rect2D sc = new()
        {
            Offset = new()
            {
                X = scissor.Origin.X,
                Y = scissor.Origin.Y
            },
            Extent = new()
            {
                Width = (uint)scissor.Size.X,
                Height = (uint)scissor.Size.Y
            }
        };

        Context.Vk.CmdSetScissor(CommandBuffer, slot, 1, &sc);
    }

    public override void SetScissorRectangles(Rectangle<int>[] scissors)
    {
        Rect2D[] scs = scissors.Select(static item => new Rect2D
        {
            Offset = new()
            {
                X = item.Origin.X,
                Y = item.Origin.Y
            },
            Extent = new()
            {
                Width = (uint)item.Size.X,
                Height = (uint)item.Size.Y
            }
        }).ToArray();

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

    public override void SetVertexBuffers(Buffer[] buffers, int[] offsets)
    {
        Context.Vk.CmdBindVertexBuffers(CommandBuffer,
                                        0,
                                        (uint)buffers.Length,
                                        buffers.Select(static item => item.VK().Buffer).ToArray(),
                                        offsets.Select(static item => (ulong)item).ToArray());
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

        foreach (VKTexture texture in vkResourceSet.SampledImages.Cast<VKTexture>())
        {
            texture.TransitionLayout(CommandBuffer, ImageLayout.ShaderReadOnlyOptimal);
        }

        foreach (VKTexture texture in vkResourceSet.StorageImages.Cast<VKTexture>())
        {
            texture.TransitionLayout(CommandBuffer, ImageLayout.General);
        }
    }

    public override void SetResourceSet(ResourceSet resourceSet,
                                        uint index = 0,
                                        uint[]? constantBufferOffsets = null)
    {
        VKResourceSet vkResourceSet = resourceSet.VK();

        uint[] offsets = new uint[vkResourceSet.DynamicCount];
        if (constantBufferOffsets is not null)
        {
            for (int i = 0; i < vkResourceSet.DynamicCount; i++)
            {
                offsets[i] = constantBufferOffsets[i];
            }
        }

        if (activePipeline is VKGraphicsPipeline graphicsPipeline)
        {
            Context.Vk.CmdBindDescriptorSets(CommandBuffer,
                                             PipelineBindPoint.Graphics,
                                             graphicsPipeline.PipelineLayout,
                                             index,
                                             1,
                                             in vkResourceSet.Token.Set,
                                             vkResourceSet.DynamicCount,
                                             in offsets[0]);
        }
        else if (activePipeline is VKComputePipeline computePipeline)
        {
            Context.Vk.CmdBindDescriptorSets(CommandBuffer,
                                             PipelineBindPoint.Compute,
                                             computePipeline.PipelineLayout,
                                             index,
                                             1,
                                             in vkResourceSet.Token.Set,
                                             vkResourceSet.DynamicCount,
                                             in offsets[0]);
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
        base.Destroy();

        Context.Vk.DestroyCommandPool(Context.Device, CommandPool, null);
    }
}
