using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct ElementDesc(ElementFormat format,
                          ElementSemanticType semantic,
                          uint semanticIndex,
                          int offset = ElementDesc.AppendAligned)
{
    public const int AppendAligned = -1;

    /// <summary>
    /// The format of the element.
    /// </summary>
    public ElementFormat Format = format;

    /// <summary>
    /// The type of the element.
    /// </summary>
    public ElementSemanticType Semantic = semantic;

    /// <summary>
    /// The index of the element.
    /// </summary>
    public uint SemanticIndex = semanticIndex;

    /// <summary>
    /// The element offset.
    /// </summary>
    public int Offset = offset;
}
