namespace ZenithEngine.Common.Descriptions;

public struct ResourceLayoutDesc(params LayoutElementDesc[] elements)
{
    public ResourceLayoutDesc()
    {
    }

    public LayoutElementDesc[] Elements = elements;

    public uint DynamicConstantBufferCount = (uint)elements.Count(static item => item.AllowDynamicOffset);
}
