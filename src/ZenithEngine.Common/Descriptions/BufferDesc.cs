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
    /// The byte stride of the structure.
    /// </summary>
    public uint StructureStrideInBytes;

    public static BufferDesc New(uint sizeInBytes,
                                 BufferUsage usage = BufferUsage.Dynamic,
                                 uint structureStrideInBytes = 0)
    {
        return new()
        {
            SizeInBytes = sizeInBytes,
            Usage = usage,
            StructureStrideInBytes = structureStrideInBytes
        };
    }
}
