using ZenithEngine.Common.Enums;

namespace ZenithEngine.DirectX12;

internal readonly struct DXResourceBinding(ShaderStages stages,
                                           ResourceType type,
                                           uint[] indices)
{
    public readonly ShaderStages Stages = stages;

    public readonly ResourceType Type = type;

    public readonly uint[] Indices = indices;
}