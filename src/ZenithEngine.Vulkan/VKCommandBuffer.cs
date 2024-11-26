﻿using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal class VKCommandBuffer : CommandBuffer
{
    public VkCommandBuffer CommandBuffer;

    public VKCommandBuffer(GraphicsContext context,
                           CommandProcessor processor) : base(context, processor)
    {
    }

    public override void Begin()
    {
        throw new NotImplementedException();
    }

    public override void BeginRendering(FrameBuffer frameBuffer, ClearValue clearValue)
    {
        throw new NotImplementedException();
    }

    public override BottomLevelAS BuildAccelerationStructure(ref readonly BottomLevelASDesc desc)
    {
        throw new NotImplementedException();
    }

    public override TopLevelAS BuildAccelerationStructure(ref readonly TopLevelASDesc desc)
    {
        throw new NotImplementedException();
    }

    public override void CopyBuffer(Buffer source, Buffer destination, uint sourceSizeInBytes, uint destinationOffsetInBytes = 0)
    {
        throw new NotImplementedException();
    }

    public override void CopyTexture(Texture source, TextureRegion sourceRegion, Texture destination, TextureRegion destinationRegion)
    {
        throw new NotImplementedException();
    }

    public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
    {
        throw new NotImplementedException();
    }

    public override void DispatchRays(uint width, uint height, uint depth)
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

    public override void DrawInstanced(uint vertexCountPerInstance, uint instanceCount, uint startVertexLocation = 0, uint startInstanceLocation = 0)
    {
        throw new NotImplementedException();
    }

    public override void DrawInstancedIndirect(Buffer argBuffer, uint offset, uint drawCount, uint stride)
    {
        throw new NotImplementedException();
    }

    public override void End()
    {
        throw new NotImplementedException();
    }

    public override void EndRendering()
    {
        throw new NotImplementedException();
    }

    public override void GenerateMipmaps(Texture texture)
    {
        throw new NotImplementedException();
    }

    public override void Reset()
    {
        throw new NotImplementedException();
    }

    public override void ResolveTexture(Texture source, TexturePosition sourcePosition, Texture destination, TexturePosition destinationPosition)
    {
        throw new NotImplementedException();
    }

    public override void SetGraphicsPipeline(GraphicsPipeline pipeline)
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

    public override void SetScissorRectangle(uint slot, Rectangle<int> scissor)
    {
        throw new NotImplementedException();
    }

    public override void SetScissorRectangles(Rectangle<int>[] scissors)
    {
        throw new NotImplementedException();
    }

    public override void SetVertexBuffer(uint slot, Buffer buffer, uint offset = 0)
    {
        throw new NotImplementedException();
    }

    public override void SetVertexBuffers(Buffer[] buffers, int[] offsets)
    {
        throw new NotImplementedException();
    }

    public override void SetViewport(uint slot, Viewport viewport)
    {
        throw new NotImplementedException();
    }

    public override void SetViewports(Viewport[] viewports)
    {
        throw new NotImplementedException();
    }

    public override void TransitionTexture(Texture texture, TextureUsage usage)
    {
        throw new NotImplementedException();
    }

    public override void UpdateAccelerationStructure(ref TopLevelAS tlas, ref readonly TopLevelASDesc newDesc)
    {
        throw new NotImplementedException();
    }

    public override void UpdateBuffer(Buffer buffer, nint source, uint sourceSizeInBytes, uint destinationOffsetInBytes = 0)
    {
        throw new NotImplementedException();
    }

    public override void UpdateTexture(Texture texture, nint source, uint sourceSizeInBytes, TextureRegion region)
    {
        throw new NotImplementedException();
    }

    protected override void DebugName(string name)
    {
        throw new NotImplementedException();
    }

    protected override void Destroy()
    {
        throw new NotImplementedException();
    }
}
