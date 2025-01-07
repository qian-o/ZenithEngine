using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct BufferDesc
{
    /// <summary>
    /// The desired capacity, in bytes.
    /// </summary>
    public uint SizeInBytes;

    /// <summary>
    /// Indicates the intended use of the buffer.
    /// </summary>
    public BufferUsage Usage;

    /// <summary>
    /// The size of each element in the buffer structure, in bytes.
    /// </summary>
    public uint StructureByteStride;

    public static BufferDesc Default(uint sizeInBytes,
                                     BufferUsage usage = BufferUsage.Dynamic,
                                     uint structureByteStride = 0)
    {
        return new()
        {
            SizeInBytes = sizeInBytes,
            Usage = usage,
            StructureByteStride = structureByteStride
        };
    }
}
