namespace Graphics.Vulkan;

public record struct ResourceSetDescription
{
    public ResourceSetDescription(ResourceLayout layout, params IBindableResource[] boundResources)
    {
        Layout = layout;
        BoundResources = boundResources;
    }

    public ResourceSetDescription(ResourceLayout layout, bool isBindless, params IBindableResource[] boundResources)
    {
        Layout = layout;
        IsBindless = isBindless;
        BoundResources = boundResources;
    }

    /// <summary>
    /// Describes the number of resources and the layout.
    /// </summary>
    public ResourceLayout Layout { get; set; }

    /// <summary>
    /// Bound resources.
    /// Resource count and types must match the descriptions in Layout.
    /// </summary>
    public IBindableResource[] BoundResources { get; set; }

    /// <summary>
    /// Whether the resource set is bindless and can be updated dynamically.
    /// When this property is true, the contents of BoundResources are ignored.
    /// </summary>
    public bool IsBindless { get; set; }
}
