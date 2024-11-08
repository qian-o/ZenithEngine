using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct LayoutDesc
{
    public ElementDesc[] Elements { get; set; }

    public VertexStepFunction StepFunction { get; set; }

    public uint StepRate { get; set; }

    public uint Stride { get; set; }

    public static LayoutDesc Default(VertexStepFunction stepFunction = VertexStepFunction.PerVertexData,
                                     uint stepRate = 0,
                                     params ElementDesc[] elements)
    {
        uint stride = 0;
        for (int i = 0; i < elements.Length; i++)
        {
            elements[i].Offset = stride;

            stride += GetFormatSizeInBytes(elements[i].Format);
        }

        return new()
        {
            Elements = elements,
            StepFunction = stepFunction,
            StepRate = stepRate,
            Stride = stride
        };
    }

    private static uint GetFormatSizeInBytes(ElementFormat format)
    {
        return format switch
        {
            ElementFormat.UByte1 or
            ElementFormat.Byte1 or
            ElementFormat.UByte1Normalized or
            ElementFormat.Byte1Normalized => 1u,

            ElementFormat.UByte2 or
            ElementFormat.Byte2 or
            ElementFormat.UByte2Normalized or
            ElementFormat.Byte2Normalized or
            ElementFormat.UShort1 or
            ElementFormat.Short1 or
            ElementFormat.UShort1Normalized or
            ElementFormat.Short1Normalized or
            ElementFormat.Half1 => 2u,

            ElementFormat.UByte3 or
            ElementFormat.Byte3 or
            ElementFormat.UByte3Normalized or
            ElementFormat.Byte3Normalized or
            ElementFormat.Half3 => 3u,

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
            ElementFormat.Int1 => 4u,

            ElementFormat.UShort3 or
            ElementFormat.Short3 or
            ElementFormat.UShort3Normalized or
            ElementFormat.Short3Normalized => 6u,

            ElementFormat.UShort4 or
            ElementFormat.Short4 or
            ElementFormat.UShort4Normalized or
            ElementFormat.Short4Normalized or
            ElementFormat.Half4 or
            ElementFormat.Float2 or
            ElementFormat.UInt2 or
            ElementFormat.Int2 => 8u,

            ElementFormat.Float3 or
            ElementFormat.UInt3 or
            ElementFormat.Int3 => 12u,

            ElementFormat.Float4 or
            ElementFormat.UInt4 or
            ElementFormat.Int4 => 16u,

            _ => throw new InvalidOperationException("VertexElementFormat doesn't supported.")
        };
    }
}
