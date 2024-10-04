namespace Graphics.Core;

public enum ResourceKind : byte
{
    /// <summary>
    /// Bind as uniform buffer.
    /// </summary>
    UniformBuffer,

    /// <summary>
    /// Bind as read-write storage buffer.
    /// </summary>
    StorageBuffer,

    /// <summary>
    /// Bind as sampled texture.
    /// </summary>
    SampledImage,

    /// <summary>
    /// Bind as read-write texture.
    /// </summary>
    StorageImage,

    /// <summary>
    /// Bind as sampler.
    /// </summary>
    Sampler,

    /// <summary>
    /// Bind as ray tracing acceleration structure.
    /// </summary>
    AccelerationStructure
}
