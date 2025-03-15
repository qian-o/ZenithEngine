using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace Common;

[StructLayout(LayoutKind.Explicit)]
public struct Material
{
    [FieldOffset(0)]
    public bool IsLight;

    [FieldOffset(4)]
    public Vector3D<float> Albedo;

    [FieldOffset(16)]
    public Vector3D<float> Emission;
}
