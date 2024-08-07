﻿using Graphics.Core;

namespace Graphics.Vulkan;

public readonly record struct VertexElementDescription
{
    public VertexElementDescription(string name, VertexElementFormat format, uint offset)
    {
        Name = name;
        Format = format;
        Offset = offset;
    }

    public VertexElementDescription(string name, VertexElementFormat format) : this(name, format, 0)
    {
    }

    /// <summary>
    /// The name of the element.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The format of the element.
    /// </summary>
    public VertexElementFormat Format { get; init; }

    /// <summary>
    /// The offset in bytes from the beginning of the vertex.
    /// </summary>
    public uint Offset { get; init; }
}
