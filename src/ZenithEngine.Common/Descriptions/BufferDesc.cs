using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct BufferDesc
{
    /// <summary>
    /// The desired capacity, in bytes.
    /// </summary>
    public uint SizeInBytes { get; set; }

    /// <summary>
    /// Indicates the intended use of the buffer.
    /// </summary>
    public BufferUsage Usage { get; set; }

    public static BufferDesc Default(uint sizeInBytes, BufferUsage usage = BufferUsage.Dynamic)
    {
        return new()
        {
            SizeInBytes = sizeInBytes,
            Usage = usage
        };
    }
}
