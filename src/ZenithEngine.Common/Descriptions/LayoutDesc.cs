using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct LayoutDesc
{
    /// <summary>
    /// A array of individual vertex elements comprising a single vertex.
    /// </summary>
    public ElementDesc[] Elements { get; set; }

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

    public static LayoutDesc Default(VertexStepFunction stepFunction = VertexStepFunction.PerVertexData,
                                     uint stepRate = 0,
                                     uint stride = 0,
                                     params ElementDesc[] elements)
    {
        return new()
        {
            Elements = elements,
            StepFunction = stepFunction,
            StepRate = stepRate,
            Stride = stride
        };
    }

    public LayoutDesc Add(ElementDesc element)
    {
        if (element.Offset == ElementDesc.AppendAligned)
        {
            element.Offset = (int)Stride;
        }

        Elements = [.. Elements, element];

        Stride += Utils.GetFormatSizeInBytes(element.Format);

        return this;
    }
}
