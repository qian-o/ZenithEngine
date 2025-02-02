using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.Maths;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXCommandBuffer : CommandBuffer
{
    public ComPtr<ID3D12CommandAllocator> CommandAllocator;
    public ComPtr<ID3D12GraphicsCommandList> CommandList;

    private readonly DXDescriptorTableAllocator? cbvSrvUavAllocator;
    private readonly DXDescriptorTableAllocator? samplerAllocator;
    private readonly ComPtr<ID3D12DescriptorHeap>[]? descriptorHeaps;

    private FrameBuffer? activeFrameBuffer;
    private Pipeline? activePipeline;

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

        if (ProcessorType is not CommandProcessorType.Copy)
        {
            cbvSrvUavAllocator = new(Context,
                                     DescriptorHeapType.CbvSrvUav,
                                     (Utils.CbvCount + Utils.SrvCount + Utils.UavCount) * 10);

            samplerAllocator = new(Context,
                                   DescriptorHeapType.Sampler,
                                   Utils.SmpCount * 10);

            descriptorHeaps =
            [
                cbvSrvUavAllocator.CpuHeap,
                samplerAllocator.CpuHeap
            ];
        }
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    #region Command Buffer Management
    public override void Begin()
    {
        if (descriptorHeaps is not null)
        {
            fixed (ID3D12DescriptorHeap** heaps = descriptorHeaps[0])
            {
                CommandList.SetDescriptorHeaps(2, heaps);
            }
        }
    }

    public override void End()
    {
        CommandList.Close();

        activeFrameBuffer = null;
        activePipeline = null;
    }

    public override void Reset()
    {
        CommandAllocator.Reset();
        CommandList.Reset(CommandAllocator, (ID3D12PipelineState*)null);

        cbvSrvUavAllocator?.Reset();
        samplerAllocator?.Reset();

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
        DXBuffer src = source.DX();
        DXBuffer dst = destination.DX();

        ResourceStates srcOldState = src.State;
        ResourceStates dstOldState = dst.State;

        src.TransitionState(CommandList, ResourceStates.CopySource);
        dst.TransitionState(CommandList, ResourceStates.CopyDest);

        CommandList.CopyBufferRegion(dst.Resource,
                                     destinationOffsetInBytes,
                                     src.Resource,
                                     sourceOffsetInBytes,
                                     sizeInBytes);

        src.TransitionState(CommandList, srcOldState);
        dst.TransitionState(CommandList, dstOldState);
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

        DXTexture dxTexture = texture.DX();

        ResourceStates old = dxTexture[region.Position.MipLevel,
                                       region.Position.ArrayLayer,
                                       region.Position.Face];

        dxTexture.TransitionState(CommandList,
                                  region.Position.MipLevel,
                                  1,
                                  region.Position.ArrayLayer,
                                  1,
                                  region.Position.Face,
                                  1,
                                  ResourceStates.CopyDest);

        TextureCopyLocation srcLocation = new()
        {
            PResource = temporary.DX().Resource,
            Type = TextureCopyType.PlacedFootprint,
            PlacedFootprint = new()
            {
                Offset = 0,
                Footprint = new()
                {
                    Format = DXFormats.GetFormat(dxTexture.Desc.Format),
                    Width = region.Width,
                    Height = region.Height,
                    Depth = region.Depth,
                    RowPitch = dxTexture.GetRowPitch(region.Position.MipLevel,
                                                     region.Position.ArrayLayer,
                                                     region.Position.Face)
                }
            }
        };

        TextureCopyLocation dstLocation = new()
        {
            PResource = dxTexture.Resource,
            Type = TextureCopyType.SubresourceIndex,
            SubresourceIndex = DXHelpers.GetDepthOrArrayIndex(dxTexture.Desc,
                                                              region.Position.MipLevel,
                                                              region.Position.ArrayLayer,
                                                              region.Position.Face)
        };

        CommandList.CopyTextureRegion(&dstLocation,
                                      region.Position.X,
                                      region.Position.Y,
                                      region.Position.Z,
                                      &srcLocation,
                                      (Box*)null);

        dxTexture.TransitionState(CommandList,
                                  region.Position.MipLevel,
                                  1,
                                  region.Position.ArrayLayer,
                                  1,
                                  region.Position.Face,
                                  1,
                                  old);
    }

    public override void CopyTexture(Texture source,
                                     TexturePosition sourcePosition,
                                     Texture destination,
                                     TexturePosition destinationPosition,
                                     uint width,
                                     uint height,
                                     uint depth)
    {
        DXTexture src = source.DX();
        DXTexture dst = destination.DX();

        ResourceStates srcOldState = src[sourcePosition.MipLevel,
                                         sourcePosition.ArrayLayer,
                                         sourcePosition.Face];
        ResourceStates dstOldState = dst[destinationPosition.MipLevel,
                                         destinationPosition.ArrayLayer,
                                         destinationPosition.Face];

        src.TransitionState(CommandList,
                            sourcePosition.MipLevel,
                            1,
                            sourcePosition.ArrayLayer,
                            1,
                            sourcePosition.Face,
                            1,
                            ResourceStates.CopySource);

        dst.TransitionState(CommandList,
                            destinationPosition.MipLevel,
                            1,
                            destinationPosition.ArrayLayer,
                            1,
                            destinationPosition.Face,
                            1,
                            ResourceStates.CopyDest);

        TextureCopyLocation srcLocation = new()
        {
            PResource = src.Resource,
            Type = TextureCopyType.SubresourceIndex,
            SubresourceIndex = DXHelpers.GetDepthOrArrayIndex(src.Desc,
                                                              sourcePosition.MipLevel,
                                                              sourcePosition.ArrayLayer,
                                                              sourcePosition.Face)
        };

        TextureCopyLocation dstLocation = new()
        {
            PResource = dst.Resource,
            Type = TextureCopyType.SubresourceIndex,
            SubresourceIndex = DXHelpers.GetDepthOrArrayIndex(dst.Desc,
                                                              destinationPosition.MipLevel,
                                                              destinationPosition.ArrayLayer,
                                                              destinationPosition.Face)
        };

        Box box = new(sourcePosition.X,
                      sourcePosition.Y,
                      sourcePosition.Z,
                      sourcePosition.X + width,
                      sourcePosition.Y + height,
                      sourcePosition.Z + depth);

        CommandList.CopyTextureRegion(&dstLocation,
                                      destinationPosition.X,
                                      destinationPosition.Y,
                                      destinationPosition.Z,
                                      &srcLocation,
                                      &box);

        src.TransitionState(CommandList,
                            sourcePosition.MipLevel,
                            1,
                            sourcePosition.ArrayLayer,
                            1,
                            sourcePosition.Face,
                            1,
                            srcOldState);

        dst.TransitionState(CommandList,
                            destinationPosition.MipLevel,
                            1,
                            destinationPosition.ArrayLayer,
                            1,
                            destinationPosition.Face,
                            1,
                            dstOldState);
    }

    public override void ResolveTexture(Texture source,
                                        TexturePosition sourcePosition,
                                        Texture destination,
                                        TexturePosition destinationPosition)
    {
        DXTexture src = source.DX();
        DXTexture dst = destination.DX();

        ResourceStates srcOldState = src[sourcePosition.MipLevel,
                                         sourcePosition.ArrayLayer,
                                         sourcePosition.Face];
        ResourceStates dstOldState = dst[destinationPosition.MipLevel,
                                         destinationPosition.ArrayLayer,
                                         destinationPosition.Face];

        src.TransitionState(CommandList,
                            sourcePosition.MipLevel,
                            1,
                            sourcePosition.ArrayLayer,
                            1,
                            sourcePosition.Face,
                            1,
                            ResourceStates.ResolveSource);

        dst.TransitionState(CommandList,
                            destinationPosition.MipLevel,
                            1,
                            destinationPosition.ArrayLayer,
                            1,
                            destinationPosition.Face,
                            1,
                            ResourceStates.ResolveDest);

        CommandList.ResolveSubresource(dst.Resource,
                                       DXHelpers.GetDepthOrArrayIndex(dst.Desc,
                                                                      destinationPosition.MipLevel,
                                                                      destinationPosition.ArrayLayer,
                                                                      destinationPosition.Face),
                                       src.Resource,
                                       DXHelpers.GetDepthOrArrayIndex(src.Desc,
                                                                      sourcePosition.MipLevel,
                                                                      sourcePosition.ArrayLayer,
                                                                      sourcePosition.Face),
                                       DXFormats.GetFormat(dst.Desc.Format));

        src.TransitionState(CommandList,
                            sourcePosition.MipLevel,
                            1,
                            sourcePosition.ArrayLayer,
                            1,
                            sourcePosition.Face,
                            1,
                            srcOldState);

        dst.TransitionState(CommandList,
                            destinationPosition.MipLevel,
                            1,
                            destinationPosition.ArrayLayer,
                            1,
                            destinationPosition.Face,
                            1,
                            dstOldState);
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
        activePipeline = pipeline;
    }

    public override void SetComputePipeline(ComputePipeline pipeline)
    {
        activePipeline = pipeline;
    }

    public override void SetRayTracingPipeline(RayTracingPipeline pipeline)
    {
        activePipeline = pipeline;
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
        samplerAllocator?.Dispose();
        cbvSrvUavAllocator?.Dispose();

        CommandList.Dispose();
        CommandAllocator.Dispose();

        base.Destroy();
    }
}
