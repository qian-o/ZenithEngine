using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public abstract class CommandBuffer(GraphicsContext context,
                                    CommandProcessor processor) : GraphicsResource(context)
{
    /// <summary>
    /// Command recording period available temporary buffer allocator.
    /// </summary>
    protected BufferAllocator BufferAllocator { get; } = new(context);

    #region Command Buffer Management
    /// <summary>
    /// Begin recording commands.
    /// </summary>
    public abstract void Begin();

    /// <summary>
    /// End recording commands.
    /// </summary>
    public abstract void End();

    /// <summary>
    /// Reset the command buffer.
    /// </summary>
    public virtual void Reset()
    {
        BufferAllocator.Release();
    }

    /// <summary>
    /// Commit the commands to the Command processor.
    /// </summary>
    public virtual void Commit()
    {
        processor.CommitCommandBuffer(this);
    }
    #endregion

    #region Buffer Operations
    /// <summary>
    /// Update the buffer with the source data.
    /// </summary>
    /// <param name="buffer">The buffer to update.</param>
    /// <param name="source">Unmanaged pointer to the source data.</param>
    /// <param name="sourceSizeInBytes">Size of the source data in bytes.</param>
    /// <param name="destinationOffsetInBytes">Offset in the destination buffer to update.</param>  
    public void UpdateBuffer(Buffer buffer,
                             nint source,
                             uint sourceSizeInBytes,
                             uint destinationOffsetInBytes = 0)
    {
        Buffer temporary = BufferAllocator.Buffer(sourceSizeInBytes);

        Context.UpdateBuffer(temporary, source, sourceSizeInBytes);

        CopyBuffer(temporary, buffer, sourceSizeInBytes, 0, destinationOffsetInBytes);
    }

    /// <summary>
    /// Copy the source buffer to the destination buffer.
    /// </summary>
    /// <param name="source">The source buffer.</param>
    /// <param name="destination">The destination buffer.</param>
    /// <param name="sizeInBytes">Size of the source buffer in bytes.</param>
    /// <param name="sourceOffsetInBytes">Offset in the source buffer to copy.</param>
    /// <param name="destinationOffsetInBytes">Offset in the destination buffer to update.</param>  
    public abstract void CopyBuffer(Buffer source,
                                    Buffer destination,
                                    uint sizeInBytes,
                                    uint sourceOffsetInBytes = 0,
                                    uint destinationOffsetInBytes = 0);
    #endregion

    #region Texture Operations
    /// <summary>
    /// Update the texture with the source data.
    /// </summary>
    /// <param name="texture">The texture to update.</param>
    /// <param name="source">Unmanaged pointer to the source data.</param>
    /// <param name="sourceSizeInBytes">Size of the source data in bytes.</param>
    /// <param name="region">Region of the texture to update.</param>
    public abstract void UpdateTexture(Texture texture,
                                       nint source,
                                       uint sourceSizeInBytes,
                                       TextureRegion region);

    /// <summary>
    /// Copy the source texture to the destination texture.
    /// </summary>
    /// <param name="source">The source texture.</param>
    /// <param name="sourceRegion">Region of the source texture to copy.</param>
    /// <param name="destination">The destination texture.</param>
    /// <param name="destinationRegion">Region of the destination texture to copy.</param>
    public abstract void CopyTexture(Texture source,
                                     TextureRegion sourceRegion,
                                     Texture destination,
                                     TextureRegion destinationRegion);

    /// <summary>
    /// Generate mipmaps for the texture.
    /// </summary>
    /// <param name="texture">The texture to generate mipmaps.</param>
    public abstract void GenerateMipmaps(Texture texture);

    /// <summary>
    /// Resolve the multisampled source texture to the destination texture.
    /// </summary>
    /// <param name="source">The source texture.</param>
    /// <param name="sourcePosition">Position of the source texture.</param>
    /// <param name="destination">The destination texture.</param>
    /// <param name="destinationPosition">Position of the destination texture.</param>
    public abstract void ResolveTexture(Texture source,
                                        TexturePosition sourcePosition,
                                        Texture destination,
                                        TexturePosition destinationPosition);

    /// <summary>
    /// Transition the texture to the specified usage.
    /// </summary>
    /// <param name="texture">The texture to transition.</param>
    /// <param name="usage">The texture usage.</param>
    public abstract void TransitionTexture(Texture texture, TextureUsage usage);
    #endregion

    #region Acceleration Structure Operations
    /// <summary>
    /// Performs a bottom level acceleration structure build on the GPU.
    /// </summary>
    /// <param name="desc">The bottom level acceleration structure description.</param>
    /// <returns>The built acceleration structure.</returns>
    public abstract BottomLevelAS BuildAccelerationStructure(ref readonly BottomLevelASDesc desc);

    /// <summary>
    /// Performs a top level acceleration structure build on the GPU.
    /// </summary>
    /// <param name="desc">The top level acceleration structure description.</param>
    /// <returns>The built acceleration structure.</returns>
    public abstract TopLevelAS BuildAccelerationStructure(ref readonly TopLevelASDesc desc);

    /// <summary>
    /// Refit a top level acceleration structure on the GPU.
    /// </summary>
    /// <param name="tlas">The top level acceleration structure to refit.</param>
    /// <param name="newDesc">The new top level acceleration structure description.</param>
    public abstract void UpdateAccelerationStructure(ref TopLevelAS tlas, ref readonly TopLevelASDesc newDesc);
    #endregion

    #region Rendering Operations
    /// <summary>
    /// Begin rendering to the frame buffer.
    /// </summary>
    /// <param name="frameBuffer">Render target frame buffer.</param>
    /// <param name="clearValue">Clear value for the frame buffer.</param>
    public abstract void BeginRendering(FrameBuffer frameBuffer, ClearValue clearValue);

    /// <summary>
    /// End rendering to the frame buffer.
    /// </summary>
    public abstract void EndRendering();

    /// <summary>
    /// Set the viewport for rendering.
    /// </summary>
    /// <param name="slot">The attachment slot.</param>
    /// <param name="viewport">The viewport.</param>
    public abstract void SetViewport(uint slot, Viewport viewport);

    /// <summary>
    /// Set the viewport for rendering.
    /// </summary>
    /// <param name="viewports">Array of viewports.</param>
    public abstract void SetViewports(Viewport[] viewports);

    /// <summary>
    /// Set the scissor rectangle for rendering.
    /// </summary>
    /// <param name="slot">The attachment slot.</param>
    /// <param name="scissor">The scissor rectangle.</param>
    public abstract void SetScissorRectangle(uint slot, Rectangle<int> scissor);

    /// <summary>
    /// Set the scissor rectangles for rendering.
    /// </summary>
    /// <param name="scissors">Array of scissor rectangles.</param>
    public abstract void SetScissorRectangles(Rectangle<int>[] scissors);
    #endregion

    #region Pipeline Operations
    /// <summary>
    /// Set the graphics pipeline for command buffer.
    /// </summary>
    /// <param name="pipeline">The graphics pipeline.</param>
    public abstract void SetGraphicsPipeline(GraphicsPipeline pipeline);

    /// <summary>
    /// Set the compute pipeline for command buffer.
    /// </summary>
    /// <param name="pipeline">The compute pipeline.</param>
    public abstract void SetComputePipeline(ComputePipeline pipeline);

    /// <summary>
    /// Set the ray tracing pipeline for command buffer.
    /// </summary>
    /// <param name="pipeline">The ray tracing pipeline.</param>
    public abstract void SetRayTracingPipeline(RayTracingPipeline pipeline);
    #endregion

    #region Resource Binding Operations
    /// <summary>
    /// Set the vertex buffer for graphics pipeline.
    /// </summary>
    /// <param name="slot">The vertex buffer slot.</param>
    /// <param name="buffer">Vertex buffer.</param>
    /// <param name="offset">Offset in the vertex buffer.</param>
    public abstract void SetVertexBuffer(uint slot, Buffer buffer, uint offset = 0);

    /// <summary>
    /// Set the vertex buffers for graphics pipeline.
    /// </summary>
    /// <param name="buffers">Array of buffers.</param>
    /// <param name="offsets">Array of offsets.</param>
    public abstract void SetVertexBuffers(Buffer[] buffers, int[] offsets);

    /// <summary>
    /// Set the index buffer for graphics pipeline.
    /// </summary>
    /// <param name="buffer">Index buffer.</param>
    /// <param name="format">Index format.</param>
    /// <param name="offset">Offset in the index buffer.</param>
    public abstract void SetIndexBuffer(Buffer buffer,
                                        IndexFormat format = IndexFormat.U16Bit,
                                        uint offset = 0);

    /// <summary>
    /// Prepare resources before pipeline binding.
    /// </summary>
    /// <param name="resourceSet">The resource set.</param>
    public abstract void PrepareResources(ResourceSet resourceSet);

    /// <summary>
    /// Set the resource set for pipeline binding.
    /// </summary>
    /// <param name="resourceSet">Resource set.</param>
    /// <param name="index">The resource set index.</param>
    /// <param name="constantBufferOffsets">Array of constant buffer offsets.</param>
    public abstract void SetResourceSet(ResourceSet resourceSet,
                                        uint index,
                                        uint[]? constantBufferOffsets = null);
    #endregion

    #region Drawing Operations
    /// <summary>
    /// Draw the instanced primitives.
    /// </summary>
    /// <param name="vertexCountPerInstance">Vertex count per instance.</param>
    /// <param name="instanceCount">Instance count.</param>
    /// <param name="startVertexLocation">Start vertex location.</param>
    /// <param name="startInstanceLocation">Start instance location.</param>
    public abstract void DrawInstanced(uint vertexCountPerInstance,
                                       uint instanceCount,
                                       uint startVertexLocation = 0,
                                       uint startInstanceLocation = 0);

    /// <summary>
    /// Draw the indexed instanced primitives.
    /// </summary>
    /// <param name="argBuffer">Argument buffer.</param>
    /// <param name="offset">Offset in the argument buffer.</param>
    /// <param name="drawCount">Draw count.</param>
    /// <param name="stride">Stride in the argument buffer.</param>
    public abstract void DrawInstancedIndirect(Buffer argBuffer,
                                               uint offset,
                                               uint drawCount,
                                               uint stride);

    /// <summary>
    /// Draw the indexed primitives.
    /// </summary>
    /// <param name="indexCount">Index count.</param>
    /// <param name="startIndexLocation">Start index location.</param>
    /// <param name="baseVertexLocation">Base vertex location.</param>
    public abstract void DrawIndexed(uint indexCount,
                                     uint startIndexLocation = 0,
                                     uint baseVertexLocation = 0);

    /// <summary>
    /// Draw the indexed instanced primitives.
    /// </summary>
    /// <param name="indexCountPerInstance">Index count per instance.</param>
    /// <param name="instanceCount">Instance count.</param>
    /// <param name="startIndexLocation">Start index location.</param>
    /// <param name="baseVertexLocation">Base vertex location.</param>
    /// <param name="startInstanceLocation">Start instance location.</param>
    public abstract void DrawIndexedInstanced(uint indexCountPerInstance,
                                              uint instanceCount,
                                              uint startIndexLocation = 0,
                                              uint baseVertexLocation = 0,
                                              uint startInstanceLocation = 0);

    /// <summary>
    /// Draw the indexed instanced indirect primitives.
    /// </summary>
    /// <param name="argBuffer">Argument buffer.</param>
    /// <param name="offset">Offset in the argument buffer.</param>
    /// <param name="drawCount">Draw count.</param>
    /// <param name="stride">Stride in the argument buffer.</param>
    public abstract void DrawIndexedInstancedIndirect(Buffer argBuffer,
                                                      uint offset,
                                                      uint drawCount,
                                                      uint stride);
    #endregion

    #region Compute Operations
    /// <summary>
    /// Dispatch the compute shader.
    /// </summary>
    /// <param name="groupCountX">The number of groups dispatched in the x direction.</param>
    /// <param name="groupCountY">The number of groups dispatched in the y direction.</param>
    /// <param name="groupCountZ">The number of groups dispatched in the z direction.</param>
    public abstract void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ);
    #endregion

    #region Ray Tracing Operations
    /// <summary>
    /// Dispatches rays on the GPU.
    /// </summary>
    /// <param name="width">The width of the ray tracing output.</param>
    /// <param name="height">The height of the ray tracing output.</param>
    /// <param name="depth">The depth of the ray tracing output.</param>
    public abstract void DispatchRays(uint width, uint height, uint depth);
    #endregion

    protected override void Destroy()
    {
        BufferAllocator.Dispose();
    }
}
