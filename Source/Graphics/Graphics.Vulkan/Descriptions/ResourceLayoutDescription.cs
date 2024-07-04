using Graphics.Core;

namespace Graphics.Vulkan;

public readonly struct ResourceLayoutDescription(params ResourceLayoutElementDescription[] elements) : IEquatable<ResourceLayoutDescription>
{
    /// <summary>
    /// The array describes the elements in the resource layout.
    /// </summary>
    public ResourceLayoutElementDescription[] Elements { get; } = elements;

    public readonly bool Equals(ResourceLayoutDescription other)
    {
        return Elements.SequenceEqual(other.Elements);
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(Elements.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is ResourceLayoutDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"Elements: {string.Join(", ", Elements)}";
    }

    public static bool operator ==(ResourceLayoutDescription left, ResourceLayoutDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ResourceLayoutDescription left, ResourceLayoutDescription right)
    {
        return !(left == right);
    }
}
