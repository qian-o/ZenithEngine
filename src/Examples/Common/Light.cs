using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace Common;

[StructLayout(LayoutKind.Explicit)]
public struct Light
{
    [FieldOffset(0)]
    public LightType Type;

    [FieldOffset(4)]
    public Vector3D<float> Position;

    [FieldOffset(16)]
    public Vector3D<float> Emission;

    [FieldOffset(32)]
    public Vector3D<float> U;

    [FieldOffset(48)]
    public Vector3D<float> V;

    [FieldOffset(60)]
    public float Area;

    [FieldOffset(64)]
    public float Radius;
}
