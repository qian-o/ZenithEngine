namespace ZenithEngine.Common.Enums;

public enum ResourceType
{
    /// <summary>
    /// Bind as constant buffer. (CBV)
    /// </summary>
    ConstantBuffer,

    /// <summary>
    /// Bind as read-only storage buffer. (SRV)
    /// </summary>
    StructuredBuffer,

    /// <summary>
    /// Bind as read-write storage buffer. (UAV)
    /// </summary>
    StructuredBufferReadWrite,

    /// <summary>
    /// Bind as read-only texture. (SRV)
    /// </summary>
    Texture,

    /// <summary>
    /// Bind as read-write texture. (UAV)
    /// </summary>
    TextureReadWrite,

    /// <summary>
    /// Bind as sampler. (SMP)
    /// </summary>
    Sampler,

    /// <summary>
    /// Bind as ray tracing acceleration structure. (SRV)
    /// </summary>
    AccelerationStructure
}
