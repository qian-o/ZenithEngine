﻿namespace ZenithEngine.Common.Enums;

[Flags]
public enum AccelerationStructureGeometryOptions
{
    None = 0,

    /// <summary>
    /// Geometry is opaque.
    /// </summary>
    Opaque = 1 << 0,

    /// <summary>
    /// Geometry is no duplicate.
    /// </summary>
    NoDuplicateAnyHitInvocation = 1 << 1
}
