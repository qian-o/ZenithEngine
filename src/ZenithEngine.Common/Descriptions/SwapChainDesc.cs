﻿using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.Common.Descriptions;

public struct SwapChainDesc
{
    /// <summary>
    /// The surface to present to.
    /// </summary>
    public ISurface Surface { get; set; }

    /// <summary>
    /// The pixel format of the color target.
    /// </summary>
    public PixelFormat ColorTargetFormat { get; set; }

    /// <summary>
    /// The pixel format of the depth stencil target.
    /// </summary>
    public PixelFormat? DepthStencilTargetFormat { get; set; }

    /// <summary>
    /// Vertical synchronization.
    /// </summary>
    public bool VerticalSync { get; set; }

    public static SwapChainDesc Default(ISurface surface,
                                        PixelFormat colorTargetFormat = PixelFormat.R8G8B8A8UNorm,
                                        PixelFormat? depthStencilTargetFormat = PixelFormat.D24UNormS8UInt,
                                        bool verticalSync = false)
    {
        return new()
        {
            Surface = surface,
            ColorTargetFormat = colorTargetFormat,
            DepthStencilTargetFormat = depthStencilTargetFormat,
            VerticalSync = verticalSync
        };
    }
}
