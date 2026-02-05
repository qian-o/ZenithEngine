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
        throw new NotImplementedException();
    }

    public override void EndRendering()
    {
        EndCurrentEncoder();
    }

    public override void SetViewports(Viewport[] viewports)
    {
        throw new NotImplementedException();
    }

    public override void SetScissorRectangles(Vector2D<int>[] offsets, Vector2D<uint>[] extents)
    {
        throw new NotImplementedException();
    }

    public override void SetGraphicsPipeline(GraphicsPipeline pipeline)
    {
        throw new NotImplementedException();
    }

    public override void SetComputePipeline(ComputePipeline pipeline)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public override void SetVertexBuffers(Buffer[] buffers, uint[] offsets)
    {
        throw new NotImplementedException();
    }

    public override void SetIndexBuffer(Buffer buffer,
                                        IndexFormat format,
                                        uint offset = 0)
    {
        throw new NotImplementedException();
    }

    public override void SetResourceSet(uint slot, ResourceSet resourceSet)
    {
        throw new NotImplementedException();
    }

    public override void Draw(uint vertexCount,
                              uint instanceCount,
                              uint firstVertex,
                              uint firstInstance)
    {
        throw new NotImplementedException();
    }

    public override void DrawIndirect(Buffer argBuffer,
                                      uint offset,
                                      uint drawCount,
                                      uint stride)
    {
        throw new NotImplementedException();
    }

    public override void DrawIndexed(uint indexCount,
                                     uint instanceCount,
                                     uint firstIndex,
                                     int vertexOffset,
                                     uint firstInstance)
    {
        throw new NotImplementedException();
    }

    public override void DrawIndexedIndirect(Buffer argBuffer,
                                             uint offset,
                                             uint drawCount,
                                             uint stride)
    {
        throw new NotImplementedException();
    }

    public override void Dispatch(uint groupCountX,
                                  uint groupCountY,
                                  uint groupCountZ)
    {
        throw new NotImplementedException();
    }

    public override void DispatchIndirect(Buffer argBuffer, uint offset)
    {
        throw new NotImplementedException();
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
}
