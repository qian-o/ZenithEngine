using Graphics.Core;

namespace Graphics.Vulkan;

public struct VertexLayoutDescription(uint stride, VertexElementDescription[] elements, uint instanceStepRate) : IEquatable<VertexLayoutDescription>
{
    public VertexLayoutDescription(uint stride, params VertexElementDescription[] elements) : this(stride, elements, 0)
    {
    }

    public VertexLayoutDescription(uint stride,
                                   uint instanceStepRate,
                                   params VertexElementDescription[] elements) : this(stride, elements, instanceStepRate)
    {
    }

    public VertexLayoutDescription(params VertexElementDescription[] elements) : this(CalculateStride(elements), elements, 0)
    {
    }

    /// <summary>
    /// The number of bytes in between successive elements in the buffer.
    /// </summary>
    public uint Stride { get; set; } = stride;

    /// <summary>
    /// The vertex elements that make up this layout.
    /// </summary>
    public VertexElementDescription[] Elements { get; set; } = elements;

    /// <summary>
    /// A value controlling how often data for instances is advanced for this layout. For per-vertex elements, this value
    /// should be 0.
    /// For example, an InstanceStepRate of 3 indicates that 3 instances will be drawn with the same value for this layout. The
    /// next 3 instances will be drawn with the next value, and so on.
    /// </summary>
    public uint InstanceStepRate { get; set; } = instanceStepRate;

    public readonly bool Equals(VertexLayoutDescription other)
    {
        return Stride == other.Stride
               && Elements.SequenceEqual(other.Elements)
               && InstanceStepRate == other.InstanceStepRate;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(Stride.GetHashCode(),
                                  Elements.GetHashCode(),
                                  InstanceStepRate.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is VertexLayoutDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"Stride: {Stride}, Elements: {string.Join(", ", Elements)}, InstanceStepRate: {InstanceStepRate}";
    }

    public static bool operator ==(VertexLayoutDescription left, VertexLayoutDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(VertexLayoutDescription left, VertexLayoutDescription right)
    {
        return !(left == right);
    }

    private static uint CalculateStride(VertexElementDescription[] elements)
    {
        uint stride = 0;
        foreach (VertexElementDescription element in elements)
        {
            stride += element.Offset;
            stride += FormatSizeHelpers.GetSizeInBytes(element.Format);
        }

        return stride;
    }
}
