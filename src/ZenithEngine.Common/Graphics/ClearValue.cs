using Silk.NET.Maths;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public struct ClearValue
{
    public ClearValue(uint colorValueCount,
                      Vector4D<float>? colorValue = null,
                      float depth = 1,
                      byte stencil = 0,
                      ClearOptions options = ClearOptions.All)
    {
        ColorValues = new Vector4D<float>[colorValueCount];
        Depth = depth;
        Stencil = stencil;
        Options = options;

        if (colorValue.HasValue)
        {
            Array.Fill(ColorValues, colorValue.Value);
        }
    }

    public Vector4D<float>[] ColorValues;

    public float Depth;

    public byte Stencil;

    public ClearOptions Options;
}
