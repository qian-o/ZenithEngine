using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct ElementDesc
{
    /// <summary>
    /// The type of the element.
    /// </summary>
    public ElementSemanticType Semantic { get; set; }

    /// <summary>
    /// The index of the element.
    /// </summary>
    public uint SemanticIndex { get; set; }

    /// <summary>
    /// The format of the element.
    /// </summary>
    public ElementFormat Format { get; set; }

    /// <summary>
    /// The element offset.
    /// </summary>
    public int Offset { get; set; }

    public static ElementDesc Default(ElementSemanticType Semantic,
                                      ElementFormat format,
                                      uint semanticIndex = 0,
                                      int offset = -1)
    {
        return new()
        {
            Semantic = Semantic,
            SemanticIndex = semanticIndex,
            Format = format,
            Offset = offset
        };
    }
}
