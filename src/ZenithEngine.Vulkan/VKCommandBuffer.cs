using Silk.NET.Maths;
using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKCommandBuffer : CommandBuffer
{
    public VkCommandPool CommandPool;
    public VkCommandBuffer CommandBuffer;

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

        src.CopyTo(CommandBuffer, dst, sizeInBytes, sourceOffsetInBytes, destinationOffsetInBytes);
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
                                     TextureRegion sourceRegion,
                                     Texture destination,
                                     TextureRegion destinationRegion)
    {
        throw new NotImplementedException();
    }

    public override void GenerateMipmaps(Texture texture)
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

    public override void TransitionTexture(Texture texture, TextureUsage usage)
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

    #region Resource Preparation
    public override void PrepareResources(ResourceSet resourceSet)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Graphics Operations
    public override void BeginRendering(FrameBuffer frameBuffer, ClearValue clearValue)
    {
        throw new NotImplementedException();
    }

    public override void EndRendering()
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

    public override void SetScissorRectangle(uint slot, Rectangle<int> scissor)
    {
        throw new NotImplementedException();
    }

    public override void SetScissorRectangles(Rectangle<int>[] scissors)
    {
        throw new NotImplementedException();
    }

    public override void SetGraphicsPipeline(GraphicsPipeline pipeline)
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

    public override void SetIndexBuffer(Buffer buffer,
                                        IndexFormat format = IndexFormat.U16Bit,
                                        uint offset = 0)
    {
        throw new NotImplementedException();
    }

    public override void SetResourceSet(ResourceSet resourceSet,
                                        uint index = 0,
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
        Context.SetDebugName(ObjectType.CommandBuffer, (ulong)CommandBuffer.Handle, name);
    }

    protected override void Destroy()
    {
        base.Destroy();

        Context.Vk.DestroyCommandPool(Context.Device, CommandPool, null);
    }
}
