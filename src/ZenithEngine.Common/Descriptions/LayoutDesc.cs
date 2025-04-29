using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct LayoutDesc(VertexStepFunction stepFunction = VertexStepFunction.PerVertexData,
                         uint stepRate = 0,
                         uint stride = 0,
                         params ElementDesc[] elements)
{
    public LayoutDesc() : this(VertexStepFunction.PerVertexData, 0, 0, [])
    {
    }

    /// <summary>
    /// A array of individual vertex elements comprising a single vertex.
    /// </summary>
    public ElementDesc[] Elements = elements;

    /// <summary>
    /// The frequency with which the vertex function fetches attribute data.
    /// </summary>
    public VertexStepFunction StepFunction = stepFunction;

    /// <summary>
    /// A value controlling how often data for instances is updated for this layout.
    /// For per-vertex elements, this value should be 0.
    /// </summary>
    public uint StepRate = stepRate;

    /// <summary>
    /// The total size of an individual vertex in bytes.
    /// </summary>
    public uint Stride = stride;

    public LayoutDesc Add(ElementDesc element)
    {
        if (element.Offset is ElementDesc.AppendAligned)
        {
            element.Offset = (int)Stride;
        }

        Elements = [.. Elements, element];

        Stride += GetFormatSizeInBytes(element.Format);

        return this;
    }

    private static uint GetFormatSizeInBytes(ElementFormat format)
    {
        return format switch
        {
            ElementFormat.UByte1 or
            ElementFormat.Byte1 or
            ElementFormat.UByte1Normalized or
            ElementFormat.Byte1Normalized => 1,

            ElementFormat.UByte2 or
            ElementFormat.Byte2 or
            ElementFormat.UByte2Normalized or
            ElementFormat.Byte2Normalized or
            ElementFormat.UShort1 or
            ElementFormat.Short1 or
            ElementFormat.UShort1Normalized or
            ElementFormat.Short1Normalized or
            ElementFormat.Half1 => 2,

            ElementFormat.UByte4 or
            ElementFormat.Byte4 or
            ElementFormat.UByte4Normalized or
            ElementFormat.Byte4Normalized or
            ElementFormat.UShort2 or
            ElementFormat.Short2 or
            ElementFormat.UShort2Normalized or
            ElementFormat.Short2Normalized or
            ElementFormat.Half2 or
            ElementFormat.Float1 or
            ElementFormat.UInt1 or
            ElementFormat.Int1 => 4,

            ElementFormat.UShort4 or
            ElementFormat.Short4 or
            ElementFormat.UShort4Normalized or
            ElementFormat.Short4Normalized or
            ElementFormat.Half4 or
            ElementFormat.Float2 or
            ElementFormat.UInt2 or
            ElementFormat.Int2 => 8,

            ElementFormat.Float3 or
            ElementFormat.UInt3 or
            ElementFormat.Int3 => 12,

            ElementFormat.Float4 or
            ElementFormat.UInt4 or
            ElementFormat.Int4 => 16,

            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(format))
        };
    }
}
