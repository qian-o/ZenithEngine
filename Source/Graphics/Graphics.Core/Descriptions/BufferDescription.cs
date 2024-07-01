namespace Graphics.Core;

public struct BufferDescription(uint sizeInBytes, BufferUsage usage, uint structureByteStride, bool rawBuffer) : IEquatable<BufferDescription>
{
    public BufferDescription(uint sizeInBytes, BufferUsage usage) : this(sizeInBytes, usage, 0, false)
    {
    }

    public BufferDescription(uint sizeInBytes, BufferUsage usage, uint structureByteStride) : this(sizeInBytes, usage, structureByteStride, false)
    {
    }

    public uint SizeInBytes { get; set; } = sizeInBytes;

    public BufferUsage Usage { get; set; } = usage;

    public uint StructureByteStride { get; set; } = structureByteStride;

    public bool RawBuffer { get; set; } = rawBuffer;

    public readonly bool Equals(BufferDescription other)
    {
        return SizeInBytes == other.SizeInBytes &&
               Usage == other.Usage &&
               StructureByteStride == other.StructureByteStride &&
               RawBuffer == other.RawBuffer;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(SizeInBytes.GetHashCode(),
                                  (int)Usage,
                                  StructureByteStride.GetHashCode(),
                                  RawBuffer.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is BufferDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"SizeInBytes: {SizeInBytes}, Usage: {Usage}, StructureByteStride: {StructureByteStride}, RawBuffer: {RawBuffer}";
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
