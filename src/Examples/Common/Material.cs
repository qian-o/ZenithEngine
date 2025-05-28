using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace Common;

[StructLayout(LayoutKind.Explicit)]
public struct Material
{
    [FieldOffset(0)]
    public Vector4D<float> Albedo;

    [FieldOffset(16)]
    public Vector4D<float> Emission;

    [FieldOffset(32)]
    public Vector4D<float> Extinction;

    [FieldOffset(48)]
    public float Metallic;

    [FieldOffset(52)]
    public float Roughness;

    [FieldOffset(56)]
    public float SubSurface;

    [FieldOffset(60)]
    public float SpecularTint;

    [FieldOffset(64)]
    public float Sheen;

    [FieldOffset(68)]
    public float SheenTint;

    [FieldOffset(72)]
    public float ClearCoat;

    [FieldOffset(76)]
    public float ClearCoatGloss;

    [FieldOffset(80)]
    public float Transmission;

    [FieldOffset(84)]
    public float IOR;

    [FieldOffset(88)]
    public float AttenuationDistance;
}
