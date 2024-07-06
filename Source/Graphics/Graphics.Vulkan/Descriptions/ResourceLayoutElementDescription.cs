using Graphics.Core;

namespace Graphics.Vulkan;

public struct ResourceLayoutElementDescription(string name, ResourceKind kind, ShaderStages stages, ResourceLayoutElementOptions options) : IEquatable<ResourceLayoutElementDescription>
{
    public ResourceLayoutElementDescription(string name, ResourceKind kind, ShaderStages stages) : this(name, kind, stages, ResourceLayoutElementOptions.None)
    {
    }

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

    /// <summary>
    /// Miscellaneous resource options for this element.
    /// </summary>
    public ResourceLayoutElementOptions Options { get; set; } = options;

    public readonly bool Equals(ResourceLayoutElementDescription other)
    {
        return Name == other.Name
               && Kind == other.Kind
               && Stages == other.Stages
               && Options == other.Options;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(Name.GetHashCode(),
                                  Kind.GetHashCode(),
                                  Stages.GetHashCode(),
                                  Options.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is ResourceLayoutElementDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"Name: {Name}, Kind: {Kind}, Stages: {Stages}, Options: {Options}";
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
