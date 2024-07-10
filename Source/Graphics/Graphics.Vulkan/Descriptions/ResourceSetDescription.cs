namespace Graphics.Vulkan;

public readonly record struct ResourceSetDescription
{
    public ResourceSetDescription(ResourceLayout layout, params IBindableResource[] boundResources)
    {
        Layout = layout;
        BoundResources = boundResources;
    }

    /// <summary>
    /// Describes the number of resources and the layout.
    /// </summary>
    public ResourceLayout Layout { get; init; }

    /// <summary>
    /// Bound resources.
    /// Resource count and types must match the descriptions in Layout.
    /// </summary>
    public IBindableResource[] BoundResources { get; init; } = [];
}
