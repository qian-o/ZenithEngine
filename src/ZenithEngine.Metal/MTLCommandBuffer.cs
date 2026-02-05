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
        throw new NotImplementedException();
    }

    public override void CopyTexture(Texture source,
                                     TexturePosition sourcePosition,
                                     Texture destination,
                                     TexturePosition destinationPosition,
                                     uint width,
                                     uint height,
                                     uint depth)
    {
        throw new NotImplementedException();
    }

    public override void ResolveTexture(Texture source,
                                        TexturePosition sourcePosition,
                                        Texture destination,
                                        TexturePosition destinationPosition)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public override void SetMeshShaderPipeline(MeshShaderPipeline pipeline)
    {
        throw new NotImplementedException();
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
        // TODO: Implement argument buffer binding
        // Metal uses argument buffers for resource sets
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
        throw new NotImplementedException();
    }

    public override void DispatchMesh(uint groupCountX, uint groupCountY, uint groupCountZ)
    {
        throw new NotImplementedException();
    }

    public override void DispatchMeshIndirect(Buffer argBuffer, uint offset, uint drawCount)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public override void EndQuery(QueryHeap queryHeap, uint queryIndex)
    {
        throw new NotImplementedException();
    }

    public override void WriteTimestamp(QueryHeap queryHeap, uint queryIndex)
    {
        throw new NotImplementedException();
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
}
