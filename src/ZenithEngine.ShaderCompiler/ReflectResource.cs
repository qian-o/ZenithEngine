using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler;

public readonly struct ReflectResource(uint space,
                                       string name,
                                       uint slot,
                                       ResourceType type,
                                       ShaderStages stages,
                                       uint count)
{
    public uint Space { get; } = space;

    public string Name { get; } = name;

    public uint Slot { get; } = slot;

    public ResourceType Type { get; } = type;

    public ShaderStages Stages { get; } = stages;

    public uint Count { get; } = count;

    public LayoutElementDesc Desc => LayoutElementDesc.Default(Slot, Type, Stages, count: Count);
}
