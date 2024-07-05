using Graphics.Core;

namespace Graphics.Vulkan;

public struct VertexElementDescription(string name, VertexElementFormat format, uint offset) : IEquatable<VertexElementDescription>
{
    public VertexElementDescription(string name, VertexElementFormat format) : this(name, format, 0)
    {
    }

    /// <summary>
    /// The name of the element.
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// The format of the element.
    /// </summary>
    public VertexElementFormat Format { get; set; } = format;

    /// <summary>
    /// The offset in bytes from the beginning of the vertex.
    /// </summary>
    public uint Offset { get; set; } = offset;

    public readonly bool Equals(VertexElementDescription other)
    {
        return Name == other.Name
               && Format == other.Format
               && Offset == other.Offset;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(Name.GetHashCode(),
                                  Format.GetHashCode(),
                                  Offset.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is VertexElementDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"Name: {Name}, Format: {Format}, Offset: {Offset}";
    }

    public static bool operator ==(VertexElementDescription left, VertexElementDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(VertexElementDescription left, VertexElementDescription right)
    {
        return !(left == right);
    }
}
