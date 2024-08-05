namespace Renderer.Structs;

internal readonly record struct Primitive
{
    public Primitive(uint firstIndex, uint indexCount, int materialIndex)
    {
        FirstIndex = firstIndex;
        IndexCount = indexCount;
        MaterialIndex = materialIndex;
    }

    public uint FirstIndex { get; init; }

    public uint IndexCount { get; init; }

    public int MaterialIndex { get; init; }
}
