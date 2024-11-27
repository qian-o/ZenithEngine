﻿namespace ZenithEngine.Common.Enums;

[Flags]
public enum TextureUsage
{
    None = 0,

    /// <summary>
    /// The Texture can be used as the target of a read-only, and can be accessed from a shader.
    /// </summary>
    Sampled = 1 << 0,

    /// <summary>
    /// The Texture can be used as the target of a read-write, and can be accessed from a shader.
    /// </summary>
    Storage = 1 << 1,

    /// <summary>
    /// The Texture can be used as the color targe.
    /// </summary>
    RenderTarget = 1 << 2,

    /// <summary>
    /// The Texture can be used as the depth target.
    /// </summary>
    DepthStencil = 1 << 3
}
