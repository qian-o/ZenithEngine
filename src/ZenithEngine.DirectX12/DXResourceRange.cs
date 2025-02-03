using ZenithEngine.Common.Enums;

namespace ZenithEngine.DirectX12;

internal readonly struct DXResourceRange(ShaderStages stages,
                                         ResourceType type,
                                         uint[] indices,
                                         int dynamicOffsetIndex)
{
    public readonly ShaderStages Stages = stages;

    public readonly ResourceType Type = type;

    public readonly uint[] Indices = indices;

    public readonly int DynamicOffsetIndex = dynamicOffsetIndex;
}