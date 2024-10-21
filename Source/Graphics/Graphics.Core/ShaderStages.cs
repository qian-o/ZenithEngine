﻿namespace Graphics.Core;

[Flags]
public enum ShaderStages : ushort
{
    /// <summary>
    /// No stages.
    /// </summary>
    None,

    /// <summary>
    /// The vertex shader stage.
    /// </summary>
    Vertex = 1 << 0,

    /// <summary>
    /// The tessellation control (or hull) shader stage.
    /// </summary>
    TessellationControl = 1 << 2,

    /// <summary>
    /// The tessellation evaluation (or domain) shader stage.
    /// </summary>
    TessellationEvaluation = 1 << 3,

    /// <summary>
    /// The geometry shader stage.
    /// </summary>
    Geometry = 1 << 1,

    /// <summary>
    /// The pixel shader stage.
    /// </summary>
    Pixel = 1 << 4,

    /// <summary>
    /// The compute shader stage.
    /// </summary>
    Compute = 1 << 5,

    /// <summary>
    /// The ray generation shader stage.
    /// </summary>
    RayGeneration = 1 << 6,

    /// <summary>
    /// The miss shader stage.
    /// </summary>
    Miss = 1 << 7,

    /// <summary>
    /// The closest-hit shader stage.
    /// </summary>
    ClosestHit = 1 << 8,

    /// <summary>
    /// The any-hit shader stage.
    /// </summary>
    AnyHit = 1 << 9,

    /// <summary>
    /// The intersection shader stage.
    /// </summary>
    Intersection = 1 << 10,

    /// <summary>
    /// The callable shader stage.
    /// </summary>
    Callable = 1 << 11,

    /// <summary>
    /// The library shader stage.
    /// </summary>
    Library = 1 << 12
}
