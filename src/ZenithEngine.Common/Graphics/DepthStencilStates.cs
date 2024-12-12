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
        None = DepthStencilStateDesc.Default();
        None.DepthEnabled = false;
        None.DepthWriteEnabled = false;

        ReadWrite = DepthStencilStateDesc.Default();

        Read = DepthStencilStateDesc.Default();
        Read.DepthWriteEnabled = false;
    }
}
