using Graphics.Core;

namespace Graphics.Vulkan;

public readonly record struct ResourceLayoutElementDescription
{
    public ResourceLayoutElementDescription(string name,
                                            ResourceKind kind,
                                            ShaderStages stages,
                                            ResourceLayoutElementOptions options)
    {
        Name = name;
        Kind = kind;
        Stages = stages;
        Options = options;
    }

    public ResourceLayoutElementDescription(string name,
                                            ResourceKind kind,
                                            ShaderStages stages) : this(name, kind, stages, ResourceLayoutElementOptions.None)
    {
    }

    /// <summary>
    /// The name of the element.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The kind of resource.
    /// </summary>
    public ResourceKind Kind { get; init; }

    /// <summary>
    /// Use the resource of the shader stage.
    /// </summary>
    public ShaderStages Stages { get; init; }

    /// <summary>
    /// Miscellaneous resource options for this element.
    /// </summary>
    public ResourceLayoutElementOptions Options { get; init; }
}
