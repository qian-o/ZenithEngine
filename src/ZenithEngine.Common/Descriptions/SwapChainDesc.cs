using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.Common.Descriptions;

public struct SwapChainDesc
{
    /// <summary>
    /// The render target.
    /// </summary>
    public ISurface Target { get; set; }

    /// <summary>
    /// The pixel format of the depth stencil target.
    /// </summary>
    public PixelFormat? DepthStencilTargetFormat { get; set; }

    /// <summary>
    /// Vertical synchronization.
    /// </summary>
    public bool VerticalSync { get; set; }

    public static SwapChainDesc Default(ISurface target,
                                        PixelFormat? depthStencilTargetFormat = PixelFormat.D24UNormS8UInt,
                                        bool verticalSync = false)
    {
        return new()
        {
            Target = target,
            DepthStencilTargetFormat = depthStencilTargetFormat,
            VerticalSync = verticalSync
        };
    }
}
