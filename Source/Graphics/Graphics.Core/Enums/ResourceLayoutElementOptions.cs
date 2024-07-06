﻿namespace Graphics.Core;

[Flags]
public enum ResourceLayoutElementOptions
{
    /// <summary>
    /// No special options.
    /// </summary>
    None,

    /// <summary>
    /// Create a dynamic binding.
    /// </summary>
    DynamicBinding = 1 << 0
}
