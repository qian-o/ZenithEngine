namespace Graphics.Core;

public enum ResourceKind : byte
{
    /// <summary>
    /// Bind as uniform buffer.
    /// </summary>
    UniformBuffer,

    /// <summary>
    /// Bind as read-only storage buffer.
    /// </summary>
    StructuredBufferReadOnly,

    /// <summary>
    /// Bind as read-write storage buffer.
    /// </summary>
    StructuredBufferReadWrite,

    /// <summary>
    /// Bind as read-only texture.
    /// </summary>
    TextureReadOnly,

    /// <summary>
    /// Bind as read-write texture.
    /// </summary>
    TextureReadWrite,

    /// <summary>
    /// Bind as sampler.
    /// </summary>
    Sampler
}
