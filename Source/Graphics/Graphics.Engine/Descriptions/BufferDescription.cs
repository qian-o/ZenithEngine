using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public unsafe struct BufferDescription
{
    /// <summary>
    /// The desired capacity, in bytes.
    /// </summary>
    public uint SizeInBytes { get; set; }

    /// <summary>
    /// Indicates the intended use of the buffer.
    /// </summary>
    public BufferUsage Usage { get; set; }

    public static BufferDescription Default<T>(int length, BufferUsage usage) where T : unmanaged
    {
        return new BufferDescription()
        {
            SizeInBytes = (uint)(length * sizeof(T)),
            Usage = usage
        };
    }
}
