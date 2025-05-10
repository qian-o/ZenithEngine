using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public struct InputElement(ElementFormat format,
                           ElementSemanticType semantic,
                           uint semanticIndex,
                           int offset = InputElement.AppendAligned)
{
    public const int AppendAligned = -1;

    public InputElement() : this(ElementFormat.UByte1,
                                ElementSemanticType.Position,
                                0,
                                AppendAligned)
    {
    }

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
