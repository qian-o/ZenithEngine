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
        CullFront = RasterizerStateDesc.New(cullMode: CullMode.Front);

        CullBack = RasterizerStateDesc.New();

        None = RasterizerStateDesc.New(cullMode: CullMode.None);

        WireframeCullFront = RasterizerStateDesc.New(cullMode: CullMode.Front, fillMode: FillMode.Wireframe);

        WireframeCullBack = RasterizerStateDesc.New(fillMode: FillMode.Wireframe);

        WireframeCullNone = RasterizerStateDesc.New(cullMode: CullMode.None, fillMode: FillMode.Wireframe);
    }
}
