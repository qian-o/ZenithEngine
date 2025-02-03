﻿using Silk.NET.Direct3D12;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal class DXResourceLayout : ResourceLayout
{
    private readonly DXResourceBinding[] bindings;

    public DXResourceLayout(GraphicsContext context,
                            ref readonly ResourceLayoutDesc desc) : base(context, in desc)
    {
        bindings = new DXResourceBinding[desc.Elements.Length];

        uint index = 0;
        int dynamicOffsetIndex = 0;
        for (int i = 0; i < desc.Elements.Length; i++)
        {
            LayoutElementDesc element = desc.Elements[i];

            if (element.Type
                is ResourceType.ConstantBuffer
                or ResourceType.StructuredBuffer
                or ResourceType.StructuredBufferReadWrite && element.AllowDynamicOffset)
            {
                bindings[i] = new(element.Stages,
                                  element.Type,
                                  [.. Enumerable.Range((int)index, (int)element.Count).Select(x => (uint)x)],
                                  dynamicOffsetIndex++);
            }
            else
            {
                bindings[i] = new(element.Stages,
                                  element.Type,
                                  [.. Enumerable.Range((int)index, (int)element.Count).Select(x => (uint)x)],
                                  -1);
            }

            index += element.Count;
        }

        if (CalculateDescriptorTableRanges(0,
                                           out DescriptorRange[] cbvSrvUavRanges,
                                           out DescriptorRange[] samplerRanges))
        {
            if (cbvSrvUavRanges.Length > 0)
            {
                AllStagesRootParameterCount++;
            }

            if (samplerRanges.Length > 0)
            {
                AllStagesRootParameterCount++;
            }
        }

        foreach (ShaderStages stage in DXHelpers.GraphicsShaderStages)
        {
            if (CalculateDescriptorTableRanges(0, out cbvSrvUavRanges, out samplerRanges, stage))
            {
                if (cbvSrvUavRanges.Length > 0)
                {
                    GraphicsRootParameterCount++;
                }

                if (samplerRanges.Length > 0)
                {
                    GraphicsRootParameterCount++;
                }
            }
        }
    }

    public uint AllStagesRootParameterCount { get; }

    public uint GraphicsRootParameterCount { get; }

    public bool CalculateDescriptorTableRanges(uint registerSpace,
                                               out DescriptorRange[] cbvSrvUavRanges,
                                               out DescriptorRange[] samplerRanges,
                                               ShaderStages stage = ShaderStages.None)
    {
        List<DescriptorRange> cbvSrvUavRangesList = [];
        List<DescriptorRange> samplerRangesList = [];

        uint cbvSrvUavOffset = 0;
        uint samplerOffset = 0;
        foreach (LayoutElementDesc element in Desc.Elements)
        {
            if (stage is not ShaderStages.None && !element.Stages.HasFlag(stage))
            {
                continue;
            }

            switch (element.Type)
            {
                case ResourceType.ConstantBuffer:
                    {
                        cbvSrvUavRangesList.Add(new()
                        {
                            RangeType = DescriptorRangeType.Cbv,
                            NumDescriptors = element.Count,
                            BaseShaderRegister = element.Slot,
                            RegisterSpace = registerSpace,
                            OffsetInDescriptorsFromTableStart = cbvSrvUavOffset
                        });

                        cbvSrvUavOffset += element.Count;
                    }
                    break;
                case ResourceType.StructuredBuffer:
                case ResourceType.Texture:
                case ResourceType.AccelerationStructure:
                    {
                        cbvSrvUavRangesList.Add(new()
                        {
                            RangeType = DescriptorRangeType.Srv,
                            NumDescriptors = element.Count,
                            BaseShaderRegister = element.Slot,
                            RegisterSpace = registerSpace,
                            OffsetInDescriptorsFromTableStart = cbvSrvUavOffset
                        });

                        cbvSrvUavOffset += element.Count;
                    }
                    break;
                case ResourceType.StructuredBufferReadWrite:
                case ResourceType.TextureReadWrite:
                    {
                        cbvSrvUavRangesList.Add(new()
                        {
                            RangeType = DescriptorRangeType.Uav,
                            NumDescriptors = element.Count,
                            BaseShaderRegister = element.Slot,
                            RegisterSpace = registerSpace,
                            OffsetInDescriptorsFromTableStart = cbvSrvUavOffset
                        });

                        cbvSrvUavOffset += element.Count;
                    }
                    break;
                case ResourceType.Sampler:
                    {
                        samplerRangesList.Add(new()
                        {
                            RangeType = DescriptorRangeType.Sampler,
                            NumDescriptors = element.Count,
                            BaseShaderRegister = element.Slot,
                            RegisterSpace = registerSpace,
                            OffsetInDescriptorsFromTableStart = samplerOffset
                        });

                        samplerOffset += element.Count;
                    }
                    break;
                default:
                    throw new ZenithEngineException(ExceptionHelpers.NotSupported(element.Type));
            }
        }

        cbvSrvUavRanges = [.. cbvSrvUavRangesList];
        samplerRanges = [.. samplerRangesList];

        return cbvSrvUavRanges.Length > 0 || samplerRanges.Length > 0;
    }

    public bool CalculateResourceBindings(out DXResourceBinding[] cbvSrvUavBindings,
                                          out DXResourceBinding[] samplerBindings,
                                          ShaderStages stage = ShaderStages.None)
    {
        List<DXResourceBinding> cbvSrvUavBindingsList = [];
        List<DXResourceBinding> samplerBindingsList = [];

        foreach (DXResourceBinding indices in bindings)
        {
            if (stage is not ShaderStages.None && !indices.Stages.HasFlag(stage))
            {
                continue;
            }

            if (indices.Type is ResourceType.Sampler)
            {
                samplerBindingsList.Add(indices);
            }
            else
            {
                cbvSrvUavBindingsList.Add(indices);
            }
        }

        cbvSrvUavBindings = [.. cbvSrvUavBindingsList];
        samplerBindings = [.. samplerBindingsList];

        return cbvSrvUavBindings.Length > 0 || samplerBindings.Length > 0;
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
