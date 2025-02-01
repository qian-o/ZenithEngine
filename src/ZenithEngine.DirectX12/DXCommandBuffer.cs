using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXCommandBuffer : CommandBuffer
{
    public ComPtr<ID3D12CommandAllocator> CommandAllocator;
    public ComPtr<ID3D12GraphicsCommandList> CommandList;

    // private FrameBuffer? activeFrameBuffer;
    // private Pipeline? activePipeline;

    public DXCommandBuffer(GraphicsContext context,
                           CommandProcessor processor) : base(context, processor)
    {
        Context.Device.CreateCommandAllocator(DXFormats.GetCommandListType(ProcessorType),
                                              out CommandAllocator).ThrowIfError();

        Context.Device.CreateCommandList(0,
                                         DXFormats.GetCommandListType(ProcessorType),
                                         CommandAllocator,
                                         (ComPtr<ID3D12PipelineState>)null,
                                         out CommandList).ThrowIfError();
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    #region Command Buffer Management
    public override void Begin()
    {
        throw new NotImplementedException();
    }

    public override void End()
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Buffer Operations
    public override void CopyBuffer(Buffer source,
                                    Buffer destination,
                                    uint sizeInBytes,
                                    uint sourceOffsetInBytes = 0,
                                    uint destinationOffsetInBytes = 0)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public override void SetViewports(Viewport[] viewports)
    {
        throw new NotImplementedException();
    }

    public override void SetScissorRectangles(Vector2D<int>[] offsets, Vector2D<uint>[] extents)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Pipeline Operations
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
    #endregion

    #region Resource Binding Operations
    public override void PrepareResources(ResourceSet[] resourceSets)
    {
        throw new NotImplementedException();
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
                                        IndexFormat format = IndexFormat.UInt16,
                                        uint offset = 0)
    {
        throw new NotImplementedException();
    }

    public override void SetResourceSet(uint slot,
                                        ResourceSet resourceSet,
                                        uint[]? constantBufferOffsets = null)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Drawing Operations
    public override void DrawInstanced(uint vertexCountPerInstance,
                                       uint instanceCount,
                                       uint startVertexLocation = 0,
                                       uint startInstanceLocation = 0)
    {
        throw new NotImplementedException();
    }

    public override void DrawInstancedIndirect(Buffer argBuffer,
                                               uint offset,
                                               uint drawCount,
                                               uint stride)
    {
        throw new NotImplementedException();
    }

    public override void DrawIndexed(uint indexCount,
                                     uint startIndexLocation = 0,
                                     uint baseVertexLocation = 0)
    {
        throw new NotImplementedException();
    }

    public override void DrawIndexedInstanced(uint indexCountPerInstance,
                                              uint instanceCount,
                                              uint startIndexLocation = 0,
                                              uint baseVertexLocation = 0,
                                              uint startInstanceLocation = 0)
    {
        throw new NotImplementedException();
    }

    public override void DrawIndexedInstancedIndirect(Buffer argBuffer,
                                                      uint offset,
                                                      uint drawCount,
                                                      uint stride)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Compute Operations
    public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
    {
        throw new NotImplementedException();
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
        CommandList.SetName(name);
    }

    protected override void Destroy()
    {
        CommandList.Dispose();
        CommandAllocator.Dispose();

        base.Destroy();
    }
}
