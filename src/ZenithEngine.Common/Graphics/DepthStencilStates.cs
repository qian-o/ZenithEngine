using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public static class DepthStencilStates
{
    /// <summary>
    /// Depth disabled.
    /// </summary>
    public static readonly DepthStencilStateDesc None;

    /// <summary>
    /// Depth enable and write mask enable.
    /// </summary>
    public static readonly DepthStencilStateDesc ReadWrite;

    /// <summary>
    /// Depth enabled, but write mask is zero.
    /// </summary>
    public static readonly DepthStencilStateDesc Read;

    static DepthStencilStates()
    {
        None = new(depthEnabled: false, depthWriteEnabled: false);

        ReadWrite = new();

        Read = new(depthWriteEnabled: false);
    }
}
