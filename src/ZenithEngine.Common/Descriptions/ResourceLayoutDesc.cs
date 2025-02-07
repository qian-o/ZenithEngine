namespace ZenithEngine.Common.Descriptions;

public struct ResourceLayoutDesc
{
    public LayoutElementDesc[] Elements;

    public uint DynamicConstantBufferCount;

    public static ResourceLayoutDesc New(params LayoutElementDesc[] elements)
    {
        return new()
        {
            Elements = elements,
            DynamicConstantBufferCount = (uint)elements.Count(static item => item.AllowDynamicOffset)
        };
    }
}
