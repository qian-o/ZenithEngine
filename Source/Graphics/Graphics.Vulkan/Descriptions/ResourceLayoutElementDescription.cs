using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct ResourceLayoutElementDescription
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
    public string Name { get; set; }

    /// <summary>
    /// The kind of resource.
    /// </summary>
    public ResourceKind Kind { get; set; }

    /// <summary>
    /// Use the resource of the shader stage.
    /// </summary>
    public ShaderStages Stages { get; set; }

    /// <summary>
    /// Miscellaneous resource options for this element.
    /// </summary>
    public ResourceLayoutElementOptions Options { get; set; }
}
