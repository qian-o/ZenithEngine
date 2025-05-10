using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.ShaderCompiler;

public readonly struct ShaderBinding(uint space, ResourceElementDesc desc)
{
    public uint Space { get; } = space;

    public ResourceElementDesc Desc { get; } = desc;
}
