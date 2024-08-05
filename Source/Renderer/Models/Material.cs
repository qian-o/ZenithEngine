using System.Numerics;
using Renderer.Enums;

namespace Renderer.Models;

internal sealed class Material(string name,
                               Vector4 baseColorFactor,
                               uint baseColorTextureIndex,
                               uint baseColorSamplerIndex,
                               uint normalTextureIndex,
                               uint normalSamplerIndex,
                               AlphaMode alphaMode,
                               float alphaCutoff,
                               bool doubleSided)
{
    public string Name { get; } = name;

    public Vector4 BaseColorFactor { get; } = baseColorFactor;

    public uint BaseColorTextureIndex { get; } = baseColorTextureIndex;

    public uint BaseColorSamplerIndex { get; } = baseColorSamplerIndex;

    public uint NormalTextureIndex { get; } = normalTextureIndex;

    public uint NormalSamplerIndex { get; } = normalSamplerIndex;

    public AlphaMode AlphaMode { get; } = alphaMode;

    public float AlphaCutoff { get; } = alphaCutoff;

    public bool DoubleSided { get; } = doubleSided;
}
