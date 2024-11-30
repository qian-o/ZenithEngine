using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct LayoutElementDesc
{
    /// <summary>
    /// The element binding.
    /// </summary>
    public uint Binding { get; set; }

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

    /// <summary>
    /// For regular resources, this value is 1, for array resources, this value is the size of the array.
    /// In Bindless mode, this value is the maximum size of the array.
    /// </summary>
    public uint Count { get; set; }

    /// <summary>
    /// If it is greater than 0, it overrides the size of this resource (in bytes).
    /// Only valid on Buffers.
    /// </summary>
    public uint Size { get; set; }

    public static LayoutElementDesc Default(uint binding,
                                            ResourceType type,
                                            ShaderStages stages,
                                            ElementOptions options = ElementOptions.None,
                                            uint count = 1,
                                            uint size = 0)
    {
        return new()
        {
            Binding = binding,
            Type = type,
            Stages = stages,
            Options = options,
            Count = count,
            Size = size
        };
    }
}
