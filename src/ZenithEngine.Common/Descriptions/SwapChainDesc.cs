using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.Common.Descriptions;

public struct SwapChainDesc(ISurface surface,
                            PixelFormat colorTargetFormat = PixelFormat.R8G8B8A8UNorm,
                            PixelFormat? depthStencilTargetFormat = PixelFormat.D24UNormS8UInt,
                            bool verticalSync = false)
{
    public SwapChainDesc()
    {
    }

    /// <summary>
    /// The surface to present to.
    /// </summary>
    public ISurface Surface = surface;

    /// <summary>
    /// The pixel format of the color target.
    /// </summary>
    public PixelFormat ColorTargetFormat = colorTargetFormat;

    /// <summary>
    /// The pixel format of the depth stencil target.
    /// </summary>
    public PixelFormat? DepthStencilTargetFormat = depthStencilTargetFormat;

    /// <summary>
    /// Vertical synchronization.
    /// </summary>
    public bool VerticalSync = verticalSync;
}
