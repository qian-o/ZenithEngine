﻿using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct LayoutDesc
{
    /// <summary>
    /// A collection of individual vertex elements comprising a single vertex.
    /// </summary>
    public List<ElementDesc> Elements { get; set; }

    /// <summary>
    /// The frequency with which the vertex function fetches attribute data.
    /// </summary>
    public VertexStepFunction StepFunction { get; set; }

    /// <summary>
    /// A value controlling how often data for instances is updated for this layout.
    /// For per-vertex elements, this value should be 0.
    /// </summary>
    public uint StepRate { get; set; }

    /// <summary>
    /// The total size of an individual vertex in bytes.
    /// </summary>
    public uint Stride { get; set; }

    public LayoutDesc Add(ElementDesc element)
    {
        if (element.Offset == ElementDesc.AppendAligned)
        {
            element.Offset = (int)Stride;
        }

        Elements.Add(element);
        Stride += GetFormatSizeInBytes(element.Format);

        return this;
    }

    public static LayoutDesc Default(VertexStepFunction stepFunction = VertexStepFunction.PerVertexData,
                                     uint stepRate = 0)
    {
        return new()
        {
            Elements = [],
            StepFunction = stepFunction,
            StepRate = stepRate,
            Stride = 0
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
