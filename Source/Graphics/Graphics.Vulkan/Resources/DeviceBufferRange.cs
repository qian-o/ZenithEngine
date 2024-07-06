using Graphics.Core;

namespace Graphics.Vulkan;

public struct DeviceBufferRange(DeviceBuffer buffer, uint offset, uint sizeInBytes) : IBindableResource, IEquatable<DeviceBufferRange>
{
    /// <summary>
    /// The buffer that this range is within.
    /// </summary>
    public DeviceBuffer Buffer { get; set; } = buffer;

    /// <summary>
    /// The offset, in bytes, from the beginning of the buffer that this range starts at.
    /// </summary>
    public uint Offset { get; set; } = offset;

    /// <summary>
    /// The total number of bytes that this range encompasses.
    /// </summary>
    public uint SizeInBytes { get; set; } = sizeInBytes;

    public readonly bool Equals(DeviceBufferRange other)
    {
        return Buffer == other.Buffer
               && Offset == other.Offset
               && SizeInBytes == other.SizeInBytes;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(Buffer.GetHashCode(),
                                  Offset.GetHashCode(),
                                  SizeInBytes.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is DeviceBufferRange range && Equals(range);
    }

    public override readonly string ToString()
    {
        return $"Buffer: {Buffer}, Offset: {Offset}, SizeInBytes: {SizeInBytes}";
    }

    public static bool operator ==(DeviceBufferRange left, DeviceBufferRange right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DeviceBufferRange left, DeviceBufferRange right)
    {
        return !(left == right);
    }
}
