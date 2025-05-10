namespace ZenithEngine.Common.Descriptions;

public struct ResourceLayoutDesc(params ResourceElementDesc[] elements)
{
    public ResourceLayoutDesc() : this([])
    {
    }

    public ResourceElementDesc[] Elements = elements;
}
