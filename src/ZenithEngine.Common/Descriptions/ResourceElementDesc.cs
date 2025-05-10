using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct ResourceElementDesc(ShaderStages stages,
                                  ResourceType type,
                                  uint index,
                                  uint count = 1)
{
    public ResourceElementDesc() : this(ShaderStages.None, ResourceType.ConstantBuffer, 0, 1)
    {
    }

    /// <summary>
    /// Use the resource of the shader stage.
    /// </summary>
    public ShaderStages Stages = stages;

    /// <summary>
    /// shader resource type.
    /// </summary>
    public ResourceType Type = type;

    /// <summary>
    /// The binding index of the resource in the shader.
    /// </summary>
    public uint Index = index;

    /// <summary>
    /// For regular resources, this value is 1, for array resources, this value is the size of the array.
    /// </summary>
    public uint Count = count;
}
