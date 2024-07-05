using Graphics.Core;

namespace Graphics.Vulkan;

public struct ResourceSetDescription(ResourceLayout layout, params IBindableResource[] boundResources) : IEquatable<ResourceSetDescription>
{
    /// <summary>
    /// Describes the number of resources and the layout.
    /// </summary>
    public ResourceLayout Layout { get; set; } = layout;

    /// <summary>
    /// Bound resources.
    /// Resource count and types must match the descriptions in Layout.
    /// </summary>
    public IBindableResource[] BoundResources { get; set; } = boundResources;

    public readonly bool Equals(ResourceSetDescription other)
    {
        return Layout == other.Layout
               && BoundResources.SequenceEqual(other.BoundResources);
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(Layout.GetHashCode(), BoundResources.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is ResourceSetDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"Layout: {Layout}, BoundResources: {BoundResources}";
    }

    public static bool operator ==(ResourceSetDescription left, ResourceSetDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ResourceSetDescription left, ResourceSetDescription right)
    {
        return !(left == right);
    }
}
