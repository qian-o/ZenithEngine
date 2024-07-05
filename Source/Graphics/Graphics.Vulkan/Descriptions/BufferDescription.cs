using Graphics.Core;

namespace Graphics.Vulkan;

public struct BufferDescription(uint sizeInBytes, BufferUsage usage) : IEquatable<BufferDescription>
{
    /// <summary>
    /// The desired capacity, in bytes.
    /// </summary>
    public uint SizeInBytes { get; set; } = sizeInBytes;

    /// <summary>
    /// Indicates the intended use of the buffer.
    /// </summary>
    public BufferUsage Usage { get; set; } = usage;

    public readonly bool Equals(BufferDescription other)
    {
        return SizeInBytes == other.SizeInBytes
               && Usage == other.Usage;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(SizeInBytes.GetHashCode(), Usage.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is BufferDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"SizeInBytes: {SizeInBytes}, Usage: {Usage}";
    }

    public static bool operator ==(BufferDescription left, BufferDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BufferDescription left, BufferDescription right)
    {
        return !(left == right);
    }
}
