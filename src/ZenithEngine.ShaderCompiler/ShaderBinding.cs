using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.ShaderCompiler;

public readonly struct ShaderBinding(uint space, ResourceElementDesc desc)
{
    public uint Space { get; } = space;

    public ResourceElementDesc Desc { get; } = desc;

    public override string ToString()
    {
        return $"Space: {Space}, Index: {Desc.Index}, Count: {Desc.Count}, Type: {Desc.Type}";
    }
}
