using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct ElementDesc
{
    public const int AppendAligned = -1;

    /// <summary>
    /// The format of the element.
    /// </summary>
    public ElementFormat Format;

    /// <summary>
    /// The type of the element.
    /// </summary>
    public ElementSemanticType Semantic;

    /// <summary>
    /// The index of the element.
    /// </summary>
    public uint SemanticIndex;

    /// <summary>
    /// The element offset.
    /// </summary>
    public int Offset;

    public static ElementDesc New(ElementFormat format,
                                  ElementSemanticType Semantic,
                                  uint semanticIndex,
                                  int offset = AppendAligned)
    {
        return new()
        {
            Format = format,
            Semantic = Semantic,
            SemanticIndex = semanticIndex,
            Offset = offset
        };
    }
}
