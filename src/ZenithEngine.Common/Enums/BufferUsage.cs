namespace ZenithEngine.Common.Enums;

[Flags]
public enum BufferUsage
{
    None = 0,

    /// <summary>
    /// Indicates can be used as the source of vertex data for drawing commands.
    /// </summary>
    VertexBuffer = 1 << 0,

    /// <summary>
    /// Indicates can be used as the source of index data for drawing commands.
    /// </summary>
    IndexBuffer = 1 << 1,

    /// <summary>
    /// Indicates can be used as a constant Buffer.
    /// </summary>
    ConstantBuffer = 1 << 2,

    /// <summary>
    /// Indicates can be used as a read-only storage Buffer.
    /// </summary>
    StructuredBuffer = 1 << 3,

    /// <summary>
    /// Indicates can be used as a read-write storage Buffer.
    /// </summary>
    StructuredBufferReadWrite = 1 << 4,

    /// <summary>
    /// Indicates can be used as the source of indirect drawing information.
    /// </summary>
    IndirectBuffer = 1 << 5,

    /// <summary>
    /// Indicates will be used in a RayTracing acceleration structure.
    /// </summary>
    AccelerationStructure = 1 << 6,

    /// <summary>
    /// Indicates will be updated with new data very frequently.
    /// </summary>
    Dynamic = 1 << 7
}
