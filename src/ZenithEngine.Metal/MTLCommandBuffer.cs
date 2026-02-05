using System;
using Metal;
using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal unsafe class MTLCommandBuffer : CommandBuffer
{
    private IMTLCommandBuffer? commandBuffer;
    private IMTLRenderCommandEncoder? renderEncoder;
    private IMTLComputeCommandEncoder? computeEncoder;
    private IMTLBlitCommandEncoder? blitEncoder;

    private FrameBuffer? activeFrameBuffer;
    private Pipeline? activePipeline;
    
    private MTLBuffer? indexBuffer;
    private uint indexBufferOffset;
    private MTLIndexType indexType;
    private MTLPrimitiveType currentPrimitiveType = MTLPrimitiveType.Triangle;

    public MTLCommandBuffer(GraphicsContext context,
                           CommandProcessor processor) : base(context, processor)
    {
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public IMTLCommandBuffer? CommandBuffer => commandBuffer;

    #region Command Buffer Management
    public override void Begin()
    {
        IMTLCommandQueue queue = ProcessorType switch
        {
            CommandProcessorType.Graphics => Context.GraphicsQueue,
            CommandProcessorType.Compute => Context.ComputeQueue,
            CommandProcessorType.Copy => Context.CopyQueue,
            _ => Context.GraphicsQueue
        };

        commandBuffer = queue.CommandBuffer();
    }

    public override void End()
    {
        EndCurrentEncoder();

        activeFrameBuffer = null;
        activePipeline = null;
    }

    public override void Reset()
    {
        commandBuffer = null;
        renderEncoder = null;
        computeEncoder = null;
        blitEncoder = null;

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
        MTLBuffer src = (MTLBuffer)source;
        MTLBuffer dst = (MTLBuffer)destination;

        EnsureBlitEncoder();

        blitEncoder!.CopyFromBuffer(src.Buffer, sourceOffsetInBytes,
                                    dst.Buffer, destinationOffsetInBytes,
                                    sizeInBytes);
    }
    #endregion

    #region Texture Operations
    public override void UpdateTexture(Texture texture,
                                       nint source,
                                       uint sourceSizeInBytes,
                                       TextureRegion region)
    {
        MTLTexture mtlTexture = (MTLTexture)texture;
        
        // Create a temporary buffer to hold the source data
        Buffer temporary = BufferAllocator.Buffer(sourceSizeInBytes);
        Context.UpdateBuffer(temporary, source, sourceSizeInBytes);
        
        MTLBuffer tempBuffer = (MTLBuffer)temporary;

        EnsureBlitEncoder();

        // Calculate bytes per row and bytes per image
        uint pixelSize = GetPixelSize(texture.Desc.Format);
        uint bytesPerRow = region.Width * pixelSize;
        uint bytesPerImage = bytesPerRow * region.Height;

        // Copy from buffer to texture
        MTLOrigin origin = new(region.Position.X, region.Position.Y, region.Position.Z);
        MTLSize size = new(region.Width, region.Height, region.Depth);

        blitEncoder!.CopyFromBuffer(
            tempBuffer.Buffer,
            0,
            bytesPerRow,
            bytesPerImage,
            size,
            mtlTexture.Texture,
            region.Position.ArrayLayer,
            region.Position.MipLevel,
            origin);
    }

    public override void CopyTexture(Texture source,
                                     TexturePosition sourcePosition,
                                     Texture destination,
                                     TexturePosition destinationPosition,
                                     uint width,
                                     uint height,
                                     uint depth)
    {
        MTLTexture src = (MTLTexture)source;
        MTLTexture dst = (MTLTexture)destination;

        EnsureBlitEncoder();

        MTLOrigin srcOrigin = new(sourcePosition.X, sourcePosition.Y, sourcePosition.Z);
        MTLSize size = new(width, height, depth);
        MTLOrigin dstOrigin = new(destinationPosition.X, destinationPosition.Y, destinationPosition.Z);

        blitEncoder!.CopyFromTexture(
            src.Texture,
            sourcePosition.ArrayLayer,
            sourcePosition.MipLevel,
            srcOrigin,
            size,
            dst.Texture,
            destinationPosition.ArrayLayer,
            destinationPosition.MipLevel,
            dstOrigin);
    }

    public override void ResolveTexture(Texture source,
                                        TexturePosition sourcePosition,
                                        Texture destination,
                                        TexturePosition destinationPosition)
    {
        MTLTexture src = (MTLTexture)source;
        MTLTexture dst = (MTLTexture)destination;

        // Metal doesn't have a separate resolve command in blit encoder
        // Resolve operations are typically done at the end of a render pass
        // For now, we'll do a regular copy which works for non-MSAA textures
        
        uint width = Math.Min(src.Desc.Width, dst.Desc.Width);
        uint height = Math.Min(src.Desc.Height, dst.Desc.Height);
        uint depth = Math.Min(src.Desc.Depth, dst.Desc.Depth);

        CopyTexture(source, sourcePosition, destination, destinationPosition, width, height, depth);
    }
    #endregion

    #region Acceleration Structure Operations
    public override BottomLevelAS BuildAccelerationStructure(ref readonly BottomLevelASDesc desc)
    {
        throw new NotSupportedException("Acceleration structures (ray tracing) are not implemented in the Metal backend");
    }

    public override TopLevelAS BuildAccelerationStructure(ref readonly TopLevelASDesc desc)
    {
        throw new NotSupportedException("Acceleration structures (ray tracing) are not implemented in the Metal backend");
    }

    public override void UpdateAccelerationStructure(ref TopLevelAS tlas, ref readonly TopLevelASDesc newDesc)
    {
        throw new NotSupportedException("Acceleration structures (ray tracing) are not implemented in the Metal backend");
    }
    #endregion

    #region Rendering Operations
    public override void BeginRendering(FrameBuffer frameBuffer, ClearValue clearValue)
    {
        activeFrameBuffer = frameBuffer;
        
        MTLFrameBuffer mtlFrameBuffer = (MTLFrameBuffer)frameBuffer;
        MTLRenderPassDescriptor passDescriptor = mtlFrameBuffer.CreateRenderPassDescriptor(clearValue);

        renderEncoder = commandBuffer!.CreateRenderCommandEncoder(passDescriptor);
        passDescriptor.Dispose();
    }

    public override void EndRendering()
    {
        EndCurrentEncoder();
    }

    public override void SetViewports(Viewport[] viewports)
    {
        if (renderEncoder is null || viewports.Length == 0)
        {
            return;
        }

        // Metal supports multiple viewports, but we'll use the first one for now
        Viewport viewport = viewports[0];
        
        MTLViewport mtlViewport = new()
        {
            OriginX = viewport.X,
            OriginY = viewport.Y,
            Width = viewport.Width,
            Height = viewport.Height,
            ZNear = viewport.MinDepth,
            ZFar = viewport.MaxDepth
        };

        renderEncoder.SetViewport(mtlViewport);
    }

    public override void SetScissorRectangles(Vector2D<int>[] offsets, Vector2D<uint>[] extents)
    {
        if (renderEncoder is null || offsets.Length == 0 || extents.Length == 0)
        {
            return;
        }

        // Use the first scissor rectangle
        MTLScissorRect scissor = new()
        {
            X = (nuint)offsets[0].X,
            Y = (nuint)offsets[0].Y,
            Width = extents[0].X,
            Height = extents[0].Y
        };

        renderEncoder.SetScissorRect(scissor);
    }

    public override void SetGraphicsPipeline(GraphicsPipeline pipeline)
    {
        activePipeline = pipeline;
        
        MTLGraphicsPipeline mtlPipeline = (MTLGraphicsPipeline)pipeline;

        if (renderEncoder is not null)
        {
            renderEncoder.SetRenderPipelineState(mtlPipeline.PipelineState);

            if (mtlPipeline.DepthStencilState is not null)
            {
                renderEncoder.SetDepthStencilState(mtlPipeline.DepthStencilState);
            }

            // Set cull mode
            renderEncoder.SetCullMode(MTLFormats.GetMTLCullMode(mtlPipeline.CullMode));

            // Set front face winding
            renderEncoder.SetFrontFacingWinding(MTLFormats.GetMTLWinding(mtlPipeline.FrontFace));

            // Set primitive topology
            currentPrimitiveType = MTLFormats.GetMTLPrimitiveType(mtlPipeline.PrimitiveTopology);
        }
    }

    public override void SetComputePipeline(ComputePipeline pipeline)
    {
        activePipeline = pipeline;
        
        MTLComputePipeline mtlPipeline = (MTLComputePipeline)pipeline;

        EnsureComputeEncoder();

        computeEncoder!.SetComputePipelineState(mtlPipeline.PipelineState);
    }

    public override void SetRayTracingPipeline(RayTracingPipeline pipeline)
    {
        throw new NotSupportedException("Ray tracing pipelines are not implemented in the Metal backend");
    }

    public override void SetMeshShaderPipeline(MeshShaderPipeline pipeline)
    {
        activePipeline = pipeline;
        
        MTLMeshShaderPipeline mtlPipeline = (MTLMeshShaderPipeline)pipeline;

        if (renderEncoder is not null)
        {
            renderEncoder.SetRenderPipelineState(mtlPipeline.PipelineState);

            if (mtlPipeline.DepthStencilState is not null)
            {
                renderEncoder.SetDepthStencilState(mtlPipeline.DepthStencilState);
            }

            // Set cull mode
            renderEncoder.SetCullMode(MTLFormats.GetMTLCullMode(mtlPipeline.CullMode));

            // Set front face winding
            renderEncoder.SetFrontFacingWinding(MTLFormats.GetMTLWinding(mtlPipeline.FrontFace));

            // Set primitive topology
            currentPrimitiveType = MTLFormats.GetMTLPrimitiveType(mtlPipeline.PrimitiveTopology);
        }
    }

    public override void PrepareResources(ResourceSet[] resourceSets)
    {
        // Metal doesn't need explicit resource preparation
    }

    public override void SetVertexBuffer(uint slot, Buffer buffer, uint offset = 0)
    {
        if (renderEncoder is null)
        {
            return;
        }

        MTLBuffer mtlBuffer = (MTLBuffer)buffer;
        renderEncoder.SetVertexBuffer(mtlBuffer.Buffer, offset, slot);
    }

    public override void SetVertexBuffers(Buffer[] buffers, uint[] offsets)
    {
        if (renderEncoder is null)
        {
            return;
        }

        for (uint i = 0; i < buffers.Length; i++)
        {
            MTLBuffer mtlBuffer = (MTLBuffer)buffers[i];
            renderEncoder.SetVertexBuffer(mtlBuffer.Buffer, offsets[i], i);
        }
    }

    public override void SetIndexBuffer(Buffer buffer,
                                        IndexFormat format,
                                        uint offset = 0)
    {
        indexBuffer = (MTLBuffer)buffer;
        indexBufferOffset = offset;
        indexType = MTLFormats.GetMTLIndexType(format);
    }

    public override void SetResourceSet(uint slot, ResourceSet resourceSet)
    {
        MTLResourceSet mtlResourceSet = (MTLResourceSet)resourceSet;

        if (renderEncoder is not null)
        {
            mtlResourceSet.BindToRenderEncoder(renderEncoder, slot);
        }
        else if (computeEncoder is not null)
        {
            mtlResourceSet.BindToComputeEncoder(computeEncoder, slot);
        }
    }

    public override void Draw(uint vertexCount,
                              uint instanceCount,
                              uint firstVertex,
                              uint firstInstance)
    {
        if (renderEncoder is null)
        {
            return;
        }

        renderEncoder.DrawPrimitives(currentPrimitiveType,
                                     firstVertex,
                                     vertexCount,
                                     instanceCount,
                                     firstInstance);
    }

    public override void DrawIndirect(Buffer argBuffer,
                                      uint offset,
                                      uint drawCount,
                                      uint stride)
    {
        if (renderEncoder is null || drawCount == 0)
        {
            return;
        }

        MTLBuffer mtlBuffer = (MTLBuffer)argBuffer;
        
        // Metal doesn't have multi-draw indirect, so we need to iterate
        for (uint i = 0; i < drawCount; i++)
        {
            renderEncoder.DrawPrimitives(currentPrimitiveType,
                                        mtlBuffer.Buffer,
                                        offset + (i * stride));
        }
    }

    public override void DrawIndexed(uint indexCount,
                                     uint instanceCount,
                                     uint firstIndex,
                                     int vertexOffset,
                                     uint firstInstance)
    {
        if (renderEncoder is null || indexBuffer is null)
        {
            return;
        }

        nuint indexBufferOffsetBytes = indexBufferOffset + (firstIndex * (indexType == MTLIndexType.UInt32 ? 4u : 2u));

        renderEncoder.DrawIndexedPrimitives(currentPrimitiveType,
                                            indexCount,
                                            indexType,
                                            indexBuffer.Buffer,
                                            indexBufferOffsetBytes,
                                            instanceCount,
                                            vertexOffset,
                                            firstInstance);
    }

    public override void DrawIndexedIndirect(Buffer argBuffer,
                                             uint offset,
                                             uint drawCount,
                                             uint stride)
    {
        if (renderEncoder is null || indexBuffer is null || drawCount == 0)
        {
            return;
        }

        MTLBuffer mtlArgBuffer = (MTLBuffer)argBuffer;
        
        // Metal doesn't have multi-draw indirect, so we need to iterate
        for (uint i = 0; i < drawCount; i++)
        {
            renderEncoder.DrawIndexedPrimitives(currentPrimitiveType,
                                                indexType,
                                                indexBuffer.Buffer,
                                                indexBufferOffset,
                                                mtlArgBuffer.Buffer,
                                                offset + (i * stride));
        }
    }

    public override void Dispatch(uint groupCountX,
                                  uint groupCountY,
                                  uint groupCountZ)
    {
        EnsureComputeEncoder();

        if (computeEncoder is null)
        {
            return;
        }

        MTLSize threadgroups = new(groupCountX, groupCountY, groupCountZ);
        
        // Get thread group size from the pipeline state
        // For now, use a default size - should be obtained from shader reflection
        MTLSize threadsPerThreadgroup = new(1, 1, 1);
        
        if (activePipeline is MTLComputePipeline computePipeline)
        {
            nuint maxThreads = computePipeline.PipelineState.MaxTotalThreadsPerThreadgroup;
            threadsPerThreadgroup = new((nuint)Math.Min(maxThreads, 256), 1, 1);
        }

        computeEncoder.DispatchThreadgroups(threadgroups, threadsPerThreadgroup);
    }

    public override void DispatchIndirect(Buffer argBuffer, uint offset)
    {
        EnsureComputeEncoder();

        if (computeEncoder is null)
        {
            return;
        }

        MTLBuffer mtlBuffer = (MTLBuffer)argBuffer;
        
        // Get thread group size from pipeline state
        MTLSize threadsPerThreadgroup = new(1, 1, 1);
        
        if (activePipeline is MTLComputePipeline computePipeline)
        {
            nuint maxThreads = computePipeline.PipelineState.MaxTotalThreadsPerThreadgroup;
            threadsPerThreadgroup = new((nuint)Math.Min(maxThreads, 256), 1, 1);
        }

        computeEncoder.DispatchThreadgroups(mtlBuffer.Buffer, offset, threadsPerThreadgroup);
    }

    public override void DispatchRays(uint width, uint height, uint depth)
    {
        throw new NotSupportedException("Ray tracing (DispatchRays) is not implemented in the Metal backend");
    }

    public override void DispatchMesh(uint groupCountX, uint groupCountY, uint groupCountZ)
    {
        if (renderEncoder is null)
        {
            return;
        }

        MTLSize threadgroups = new(groupCountX, groupCountY, groupCountZ);
        
        // For mesh shaders, we dispatch threadgroups directly
        // The threadgroup size is defined in the mesh shader itself
        renderEncoder.DrawMeshThreadgroups(threadgroups, new MTLSize(1, 1, 1), new MTLSize(1, 1, 1));
    }

    public override void DispatchMeshIndirect(Buffer argBuffer, uint offset, uint drawCount)
    {
        if (renderEncoder is null || drawCount == 0)
        {
            return;
        }

        MTLBuffer mtlBuffer = (MTLBuffer)argBuffer;
        
        // Metal doesn't support multi-draw mesh indirect, iterate
        for (uint i = 0; i < drawCount; i++)
        {
            renderEncoder.DrawMeshThreadgroups(mtlBuffer.Buffer, offset + (i * 12), // 12 bytes per draw (3 uints)
                                               new MTLSize(1, 1, 1), new MTLSize(1, 1, 1));
        }
    }
    #endregion

    #region Debug Operations
    public override void BeginDebugEvent(string label)
    {
        commandBuffer?.PushDebugGroup(label);
    }

    public override void EndDebugEvent()
    {
        commandBuffer?.PopDebugGroup();
    }

    public override void InsertDebugMarker(string label)
    {
        // Metal doesn't have insert marker, use push/pop
        commandBuffer?.PushDebugGroup(label);
        commandBuffer?.PopDebugGroup();
    }

    public override void BeginQuery(QueryHeap queryHeap, uint queryIndex)
    {
        // In Metal, queries are recorded via sample boundaries
        // BeginQuery is not directly applicable - we use WriteTimestamp instead
    }

    public override void EndQuery(QueryHeap queryHeap, uint queryIndex)
    {
        // In Metal, queries are recorded via sample boundaries
        // EndQuery is not directly applicable - we use WriteTimestamp instead
    }

    public override void WriteTimestamp(QueryHeap queryHeap, uint queryIndex)
    {
        MTLQueryHeap mtlQueryHeap = (MTLQueryHeap)queryHeap;

        if (mtlQueryHeap.CounterSampleBuffer is null)
        {
            return;
        }

        // Sample the counter at this point
        if (renderEncoder is not null)
        {
            renderEncoder.SampleCounters(mtlQueryHeap.CounterSampleBuffer, queryIndex, true);
        }
        else if (computeEncoder is not null)
        {
            computeEncoder.SampleCounters(mtlQueryHeap.CounterSampleBuffer, queryIndex, true);
        }
        else if (blitEncoder is not null)
        {
            blitEncoder.SampleCounters(mtlQueryHeap.CounterSampleBuffer, queryIndex, true);
        }
    }
    #endregion

    protected override void SetName(string name)
    {
        commandBuffer?.SetLabel(name);
    }

    protected override void Destroy()
    {
        commandBuffer?.Dispose();
    }

    private void EndCurrentEncoder()
    {
        renderEncoder?.EndEncoding();
        renderEncoder?.Dispose();
        renderEncoder = null;

        computeEncoder?.EndEncoding();
        computeEncoder?.Dispose();
        computeEncoder = null;

        blitEncoder?.EndEncoding();
        blitEncoder?.Dispose();
        blitEncoder = null;
    }

    private void EnsureBlitEncoder()
    {
        if (blitEncoder is not null)
        {
            return;
        }

        EndCurrentEncoder();

        blitEncoder = commandBuffer!.BlitCommandEncoder();
    }

    private void EnsureComputeEncoder()
    {
        if (computeEncoder is not null)
        {
            return;
        }

        EndCurrentEncoder();

        computeEncoder = commandBuffer!.ComputeCommandEncoder();
    }

    private static uint GetPixelSize(PixelFormat format)
    {
        return format switch
        {
            PixelFormat.R8UNorm or PixelFormat.R8SNorm or PixelFormat.R8UInt or PixelFormat.R8SInt => 1,
            PixelFormat.R16UNorm or PixelFormat.R16SNorm or PixelFormat.R16UInt or PixelFormat.R16SInt or PixelFormat.R16Float => 2,
            PixelFormat.R8G8UNorm or PixelFormat.R8G8SNorm or PixelFormat.R8G8UInt or PixelFormat.R8G8SInt => 2,
            PixelFormat.R32UInt or PixelFormat.R32SInt or PixelFormat.R32Float => 4,
            PixelFormat.R16G16UNorm or PixelFormat.R16G16SNorm or PixelFormat.R16G16UInt or PixelFormat.R16G16SInt or PixelFormat.R16G16Float => 4,
            PixelFormat.R8G8B8A8UNorm or PixelFormat.R8G8B8A8UNormSRgb or PixelFormat.R8G8B8A8SNorm or PixelFormat.R8G8B8A8UInt or PixelFormat.R8G8B8A8SInt => 4,
            PixelFormat.B8G8R8A8UNorm or PixelFormat.B8G8R8A8UNormSRgb => 4,
            PixelFormat.R32G32UInt or PixelFormat.R32G32SInt or PixelFormat.R32G32Float => 8,
            PixelFormat.R16G16B16A16UNorm or PixelFormat.R16G16B16A16SNorm or PixelFormat.R16G16B16A16UInt or PixelFormat.R16G16B16A16SInt or PixelFormat.R16G16B16A16Float => 8,
            PixelFormat.R32G32B32UInt or PixelFormat.R32G32B32SInt or PixelFormat.R32G32B32Float => 12,
            PixelFormat.R32G32B32A32UInt or PixelFormat.R32G32B32A32SInt or PixelFormat.R32G32B32A32Float => 16,
            PixelFormat.D24UNormS8UInt or PixelFormat.D32FloatS8UInt => 4,
            _ => 4 // Default for compressed formats
        };
    }
}
