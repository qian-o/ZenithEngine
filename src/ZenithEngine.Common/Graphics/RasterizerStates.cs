using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public static class RasterizerStates
{
    /// <summary>
    /// Culls primitives with clockwise winding order.
    /// </summary>
    public static readonly RasterizerStateDesc CullFront;

    /// <summary>
    /// Culls primitives with a counterclockwise winding order.
    /// </summary>
    public static readonly RasterizerStateDesc CullBack;

    /// <summary>
    /// Does not cull primitives.
    /// </summary>
    public static readonly RasterizerStateDesc None;

    /// <summary>
    /// Culls primitives with a clockwise winding order and enables the wireframe.
    /// </summary>
    public static readonly RasterizerStateDesc WireframeCullFront;

    /// <summary>
    /// Culls primitives with a counter-clockwise winding order and enables wireframe
    /// mode.
    /// </summary>
    public static readonly RasterizerStateDesc WireframeCullBack;

    /// <summary>
    /// Do not cull primitives, and enable wireframe.
    /// </summary>
    public static readonly RasterizerStateDesc WireframeCullNone;

    static RasterizerStates()
    {
        CullFront = RasterizerStateDesc.Default();
        CullFront.CullMode = CullMode.Front;

        CullBack = RasterizerStateDesc.Default();

        None = RasterizerStateDesc.Default();
        None.CullMode = CullMode.None;

        WireframeCullFront = RasterizerStateDesc.Default();
        WireframeCullFront.CullMode = CullMode.Front;
        WireframeCullFront.FillMode = FillMode.Wireframe;

        WireframeCullBack = RasterizerStateDesc.Default();
        WireframeCullBack.FillMode = FillMode.Wireframe;

        WireframeCullNone = RasterizerStateDesc.Default();
        WireframeCullNone.CullMode = CullMode.None;
        WireframeCullNone.FillMode = FillMode.Wireframe;
    }
}
