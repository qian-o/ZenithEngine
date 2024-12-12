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
    public readonly ShaderStages Stages = stages;

    public readonly ResourceType Type = type;

    public readonly uint Slot = slot;

    public readonly uint Space = space;

    public readonly string Name = name;

    public readonly uint Count = count;

    public LayoutElementDesc Desc => LayoutElementDesc.Default(Stages, Type, Slot, count: Count);
}
