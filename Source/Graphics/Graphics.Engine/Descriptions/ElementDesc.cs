using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct ElementDesc
{
    /// <summary>
    /// The name of the element.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The format of the element.
    /// </summary>
    public ElementFormat Format { get; set; }

    /// <summary>
    /// The offset in bytes from the beginning of the vertex.
    /// </summary>
    public uint Offset { get; set; }

    public static ElementDesc Default(string name, ElementFormat format, uint offset = 0)
    {
        return new ElementDesc
        {
            Name = name,
            Format = format,
            Offset = offset
        };
    }
}
