﻿using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct LayoutElementDesc
{
    /// <summary>
    /// The slot of the element.
    /// </summary>
    public uint Slot { get; set; }

    /// <summary>
    /// shader resource type.
    /// </summary>
    public ResourceType Type { get; set; }

    /// <summary>
    /// Use the resource of the shader stage.
    /// </summary>
    public ShaderStages Stages { get; set; }

    /// <summary>
    /// Miscellaneous resource options for this element.
    /// </summary>
    public ElementOptions Options { get; set; }

    public static LayoutElementDesc Default(uint slot,
                                            ResourceType type,
                                            ShaderStages stages,
                                            ElementOptions options = ElementOptions.None)
    {
        return new()
        {
            Slot = slot,
            Type = type,
            Stages = stages,
            Options = options
        };
    }
}
