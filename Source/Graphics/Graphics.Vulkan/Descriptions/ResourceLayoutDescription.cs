namespace Graphics.Vulkan;

public record struct ResourceLayoutDescription
{
    public ResourceLayoutDescription(params ResourceLayoutElementDescription[] elements)
    {
        IsBindless = false;
        Elements = elements;
    }

    public ResourceLayoutDescription(bool isBindless, params ResourceLayoutElementDescription[] elements)
    {
        IsBindless = isBindless;
        Elements = elements;
    }

    /// <summary>
    /// Whether the resource layout is bindless.
    /// </summary>
    public bool IsBindless { get; set; }

    /// <summary>
    /// The array describes the elements in the resource layout.
    /// </summary>
    public ResourceLayoutElementDescription[] Elements { get; set; }
}
