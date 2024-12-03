using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler;

public readonly struct ReflectResource(ShaderStages stages,
                                       ResourceType type,
                                       uint slot,
                                       uint space,
                                       string name,
                                       uint count)
{
    public ShaderStages Stages { get; } = stages;

    public ResourceType Type { get; } = type;

    public uint Slot { get; } = slot;

    public uint Space { get; } = space;

    public string Name { get; } = name;

    public uint Count { get; } = count;

    public LayoutElementDesc Desc => LayoutElementDesc.Default(Stages, Type, Slot, count: Count);
}
