﻿using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct LayoutElementDesc
{
    /// <summary>
    /// Use the resource of the shader stage.
    /// </summary>
    public ShaderStages Stages;

    /// <summary>
    /// shader resource type.
    /// </summary>
    public ResourceType Type;

    /// <summary>
    /// The slot of the element.
    /// </summary>
    public uint Slot;

    /// <summary>
    /// For regular resources, this value is 1, for array resources, this value is the size of the array.
    /// </summary>
    public uint Count;

    /// <summary>
    /// This value indicates whether this resource can have a dynamic offset.
    /// </summary>
    public bool AllowDynamicOffset;

    /// <summary>
    /// If it is greater than 0, it overrides the size of this resource (in bytes).
    /// Only valid on Buffers.
    /// </summary>
    public uint Range;

    public static LayoutElementDesc New(ShaderStages stages,
                                        ResourceType type,
                                        uint slot,
                                        uint count = 1,
                                        bool allowDynamicOffset = false,
                                        uint range = 0)
    {
        return new()
        {
            Stages = stages,
            Type = type,
            Slot = slot,
            Count = count,
            AllowDynamicOffset = allowDynamicOffset,
            Range = range
        };
    }
}
