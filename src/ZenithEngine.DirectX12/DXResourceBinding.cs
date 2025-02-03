using ZenithEngine.Common.Enums;

namespace ZenithEngine.DirectX12;

internal readonly struct DXResourceBinding(ShaderStages stages,
                                           ResourceType type,
                                           uint range,
                                           uint[] indices,
                                           int dynamicOffsetIndex)
{
    public readonly ShaderStages Stages = stages;

    public readonly ResourceType Type = type;

    public readonly uint Range = range;

    public readonly uint[] Indices = indices;

    public readonly int DynamicOffsetIndex = dynamicOffsetIndex;
}