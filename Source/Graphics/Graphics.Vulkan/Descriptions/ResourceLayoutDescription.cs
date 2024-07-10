namespace Graphics.Vulkan;

public readonly record struct ResourceLayoutDescription
{
    public ResourceLayoutDescription(params ResourceLayoutElementDescription[] elements)
    {
        Elements = elements;
    }

    /// <summary>
    /// The array describes the elements in the resource layout.
    /// </summary>
    public ResourceLayoutElementDescription[] Elements { get; } = [];
}
