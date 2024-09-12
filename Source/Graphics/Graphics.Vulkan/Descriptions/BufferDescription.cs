using Graphics.Core;

namespace Graphics.Vulkan;

public record struct BufferDescription
{
    public BufferDescription(uint sizeInBytes, BufferUsage usage)
    {
        SizeInBytes = sizeInBytes;
        Usage = usage;
    }

    internal BufferDescription(uint sizeInBytes, bool isDescriptorBuffer, bool isDynamic)
    {
        SizeInBytes = sizeInBytes;
        Usage = isDynamic ? BufferUsage.Dynamic : default;
        IsDescriptorBuffer = isDescriptorBuffer;
    }

    /// <summary>
    /// The desired capacity, in bytes.
    /// </summary>
    public uint SizeInBytes { get; set; }

    /// <summary>
    /// Indicates the intended use of the buffer.
    /// </summary>
    public BufferUsage Usage { get; set; }

    /// <summary>
    /// (Internal use) Indicates whether it is a descriptor buffer.
    /// </summary>
    internal bool IsDescriptorBuffer { get; set; }
}
