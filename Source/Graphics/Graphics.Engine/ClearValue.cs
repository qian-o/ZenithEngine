using System.Numerics;
using Graphics.Engine.Enums;

namespace Graphics.Engine;

public struct ClearValue
{
    public readonly static ClearValue None = new()
    {
        ColorValues = new Vector4[1],
        Depth = 1.0f,
        Stencil = 0,
        Options = ClearOptions.None
    };

    public readonly static ClearValue Default = new()
    {
        ColorValues = new Vector4[1],
        Depth = 1.0f,
        Stencil = 0,
        Options = ClearOptions.All
    };

    public Vector4[] ColorValues { get; set; }

    public float Depth { get; set; }

    public byte Stencil { get; set; }

    public ClearOptions Options { get; set; }
}
