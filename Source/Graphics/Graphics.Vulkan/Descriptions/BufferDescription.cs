using Graphics.Core;

namespace Graphics.Vulkan;

public record struct BufferDescription
{
    public BufferDescription(uint sizeInBytes, BufferUsage usage)
    {
        SizeInBytes = sizeInBytes;
        Usage = usage;
    }

    /// <summary>
    /// The desired capacity, in bytes.
    /// </summary>
    public uint SizeInBytes { get; set; }

    /// <summary>
    /// Indicates the intended use of the buffer.
    /// </summary>
    public BufferUsage Usage { get; set; }

    public static unsafe BufferDescription VertexBuffer<T>(int length = 1, bool isDynamic = false) where T : unmanaged
    {
        return new BufferDescription((uint)(length * sizeof(T)),
                                     BufferUsage.VertexBuffer | (isDynamic ? BufferUsage.Dynamic : 0));
    }

    public static unsafe BufferDescription IndexBuffer<T>(int length = 1, bool isDynamic = false) where T : unmanaged
    {
        return new BufferDescription((uint)(length * sizeof(T)),
                                     BufferUsage.IndexBuffer | (isDynamic ? BufferUsage.Dynamic : 0));
    }

    public static unsafe BufferDescription UniformBuffer<T>(int length = 1, bool isDynamic = false) where T : unmanaged
    {
        return new BufferDescription((uint)(length * sizeof(T)),
                                     BufferUsage.UniformBuffer | (isDynamic ? BufferUsage.Dynamic : 0));
    }

    public static unsafe BufferDescription StorageBuffer<T>(int length = 1, bool isDynamic = false) where T : unmanaged
    {
        return new BufferDescription((uint)(length * sizeof(T)),
                                     BufferUsage.StorageBuffer | (isDynamic ? BufferUsage.Dynamic : 0));
    }
}
