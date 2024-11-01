using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct BufferDescription(uint sizeInBytes, BufferUsage usage)
{
    /// <summary>
    /// The desired capacity, in bytes.
    /// </summary>
    public uint SizeInBytes { get; set; } = sizeInBytes;

    /// <summary>
    /// Indicates the intended use of the buffer.
    /// </summary>
    public BufferUsage Usage { get; set; } = usage;

    public static unsafe BufferDescription Create<T>(int length, BufferUsage usage) where T : unmanaged
    {
        return new BufferDescription((uint)(length * sizeof(T)), usage);
    }
}
