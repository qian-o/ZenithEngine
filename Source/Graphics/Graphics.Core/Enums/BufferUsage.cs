namespace Graphics.Core;

[Flags]
public enum BufferUsage : byte
{
    /// <summary>
    /// Indicates can be used as the source of vertex data for drawing commands.
    /// </summary>
    VertexBuffer = 1 << 0,

    /// <summary>
    /// Indicates can be used as the source of index data for drawing commands.
    /// </summary>
    IndexBuffer = 1 << 1,

    /// <summary>
    /// Indicates can be used as a uniform Buffer.
    /// </summary>
    UniformBuffer = 1 << 2,

    /// <summary>
    /// Indicates can be used as a read-only structured Buffer.
    /// </summary>
    StructuredBufferReadOnly = 1 << 3,

    /// <summary>
    /// Indicates can be used as a read-write structured Buffer.
    /// </summary>
    StructuredBufferReadWrite = 1 << 4,

    /// <summary>
    /// Indicates can be used as the source of indirect drawing information.
    /// </summary>
    IndirectBuffer = 1 << 5,

    /// <summary>
    /// Indicates will be updated with new data very frequently.
    /// </summary>
    Dynamic = 1 << 6,

    /// <summary>
    /// Indicates will be used as a staging Buffer.
    /// </summary>
    Staging = 1 << 7
}
