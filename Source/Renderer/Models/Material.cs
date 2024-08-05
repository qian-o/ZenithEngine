using System.Numerics;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using AlphaMode = Renderer.Enums.AlphaMode;
using GltfAlphaMode = SharpGLTF.Schema2.AlphaMode;
using GltfMaterial = SharpGLTF.Schema2.Material;

namespace Renderer.Models;

internal sealed class Material
{
    public Material(GltfMaterial gltfMaterial)
    {
        Name = gltfMaterial.Name;

        if (gltfMaterial.FindChannel(KnownChannel.BaseColor.ToString()) is MaterialChannel baseColor)
        {
            BaseColorFactor = baseColor.Color;

            if (baseColor.Texture != null)
            {
                BaseColorTextureIndex = (uint)baseColor.Texture.LogicalIndex;
            }
        }

        if (gltfMaterial.FindChannel(KnownChannel.Normal.ToString()) is MaterialChannel normal)
        {
            if (normal.Texture != null)
            {
                NormalTextureIndex = (uint)normal.Texture.LogicalIndex;
            }
        }

        AlphaMode = ToAlphaMode(gltfMaterial.Alpha);
        AlphaCutoff = gltfMaterial.AlphaCutoff;
        DoubleSided = gltfMaterial.DoubleSided;
    }

    public string Name { get; }

    public Vector4 BaseColorFactor { get; }

    public uint BaseColorTextureIndex { get; }

    public uint NormalTextureIndex { get; }

    public AlphaMode AlphaMode { get; }

    public float AlphaCutoff { get; }

    public bool DoubleSided { get; }

    private static AlphaMode ToAlphaMode(GltfAlphaMode alphaMode)
    {
        return alphaMode switch
        {
            GltfAlphaMode.MASK => AlphaMode.Mask,
            GltfAlphaMode.OPAQUE => AlphaMode.Opaque,
            GltfAlphaMode.BLEND => AlphaMode.Blend,
            _ => throw new ArgumentOutOfRangeException(nameof(alphaMode), alphaMode, null)
        };
    }
}
