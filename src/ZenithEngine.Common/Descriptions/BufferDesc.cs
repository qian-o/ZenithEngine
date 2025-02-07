using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct BufferDesc(uint sizeInBytes,
                         BufferUsage usage = BufferUsage.Dynamic,
                         uint structureStrideInBytes = 0)
{
    public BufferDesc()
    {
    }

    /// <summary>
    /// The desired capacity, in bytes.
    /// </summary>
    public uint SizeInBytes = sizeInBytes;

    /// <summary>
    /// Indicates the intended use of the buffer.
    /// </summary>
    public BufferUsage Usage = usage;

    /// <summary>
    /// The byte stride of the structure.
    /// </summary>
    public uint StructureStrideInBytes = structureStrideInBytes;
}
