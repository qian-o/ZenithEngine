using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct LayoutDesc(VertexStepFunction stepFunction = VertexStepFunction.PerVertexData,
                         uint stepRate = 0,
                         uint stride = 0,
                         params ElementDesc[] elements)
{
    public LayoutDesc()
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

        Stride += Utils.GetFormatSizeInBytes(element.Format);

        return this;
    }
}
