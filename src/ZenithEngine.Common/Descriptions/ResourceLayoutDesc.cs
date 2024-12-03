namespace ZenithEngine.Common.Descriptions;

public struct ResourceLayoutDesc
{
    public LayoutElementDesc[] Elements { get; set; }

    public uint DynamicConstantBufferCount { get; set; }

    public static ResourceLayoutDesc Default(params LayoutElementDesc[] elements)
    {
        return new()
        {
            Elements = elements,
            DynamicConstantBufferCount = (uint)elements.Count(static item => item.AllowDynamicOffset)
        };
    }
}
