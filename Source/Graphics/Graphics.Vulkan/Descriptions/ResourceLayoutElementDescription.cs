using Graphics.Core;

namespace Graphics.Vulkan;

public struct ResourceLayoutElementDescription(string name, ResourceKind kind, ShaderStages stages) : IEquatable<ResourceLayoutElementDescription>
{
    /// <summary>
    /// The name of the element.
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// The kind of resource.
    /// </summary>
    public ResourceKind Kind { get; set; } = kind;

    /// <summary>
    /// Use the resource of the shader stage.
    /// </summary>
    public ShaderStages Stages { get; set; } = stages;

    public readonly bool Equals(ResourceLayoutElementDescription other)
    {
        return Name == other.Name && Kind == other.Kind && Stages == other.Stages;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Name, Kind, Stages);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is ResourceLayoutElementDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"Name: {Name}, Kind: {Kind}, Stages: {Stages}";
    }

    public static bool operator ==(ResourceLayoutElementDescription left, ResourceLayoutElementDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ResourceLayoutElementDescription left, ResourceLayoutElementDescription right)
    {
        return !(left == right);
    }
}
