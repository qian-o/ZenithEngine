namespace ZenithEngine.Common.Enums;

[Flags]
public enum TextureUsage
{
    None = 0,

    /// <summary>
    /// The Texture can be used as a shader resource.
    /// </summary>
    ShaderResource = 1 << 0,

    /// <summary>
    /// The Texture can be used as an unordered access view.
    /// </summary>
    UnorderedAccess = 1 << 1,

    /// <summary>
    /// The Texture can be used as the color targe.
    /// </summary>
    RenderTarget = 1 << 2,

    /// <summary>
    /// The Texture can be used as the depth target.
    /// </summary>
    DepthStencil = 1 << 3
}
