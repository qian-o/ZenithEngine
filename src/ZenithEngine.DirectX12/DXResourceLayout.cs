using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal class DXResourceLayout : ResourceLayout
{
    public DXResourceLayout(GraphicsContext context,
                            ref readonly ResourceLayoutDesc desc) : base(context, in desc)
    {
        foreach (ShaderStages stage in DXHelpers.GraphicsShaderStages)
        {
            if (CalculateDescriptorIndices(out _, out _, out _, out _, stage))
            {
                GraphicsRootParameterCount++;
            }
        }

        if (CalculateDescriptorIndices(out _, out _, out _, out _))
        {
            ComputeOrRayTracingRootParameterCount++;
        }
    }

    public uint GraphicsRootParameterCount { get; }

    public uint ComputeOrRayTracingRootParameterCount { get; }

    public bool CalculateDescriptorIndices(out uint[] cbvIndices,
                                           out uint[] srvIndices,
                                           out uint[] uavIndices,
                                           out uint[] samplerIndices,
                                           ShaderStages stage = ShaderStages.None)
    {
        List<uint> cbvIndicesList = [];
        List<uint> srvIndicesList = [];
        List<uint> uavIndicesList = [];
        List<uint> samplerIndicesList = [];

        uint offset = 0;
        foreach (LayoutElementDesc element in Desc.Elements)
        {
            if (stage is not ShaderStages.None && !element.Stages.HasFlag(stage))
            {
                offset += element.Count;

                continue;
            }

            switch (element.Type)
            {
                case ResourceType.ConstantBuffer:
                    AddIndices(cbvIndicesList, element.Count, ref offset);
                    break;
                case ResourceType.StructuredBuffer:
                case ResourceType.Texture:
                case ResourceType.AccelerationStructure:
                    AddIndices(srvIndicesList, element.Count, ref offset);
                    break;
                case ResourceType.StructuredBufferReadWrite:
                case ResourceType.TextureReadWrite:
                    AddIndices(uavIndicesList, element.Count, ref offset);
                    break;
                case ResourceType.Sampler:
                    AddIndices(samplerIndicesList, element.Count, ref offset);
                    break;
                default:
                    throw new ZenithEngineException(ExceptionHelpers.NotSupported(element.Type));
            }
        }

        cbvIndices = [.. cbvIndicesList];
        srvIndices = [.. srvIndicesList];
        uavIndices = [.. uavIndicesList];
        samplerIndices = [.. samplerIndicesList];

        return cbvIndices.Length + srvIndices.Length + uavIndices.Length + samplerIndices.Length > 0;

        static void AddIndices(List<uint> indices, uint count, ref uint offset)
        {
            for (uint i = 0; i < count; i++)
            {
                indices.Add(offset++);
            }
        }
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
