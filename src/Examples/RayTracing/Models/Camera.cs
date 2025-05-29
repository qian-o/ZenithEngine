using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace RayTracing.Models;

[StructLayout(LayoutKind.Explicit)]
internal struct Camera
{
    [FieldOffset(0)]
    public Vector3D<float> Position;

    [FieldOffset(16)]
    public Vector3D<float> Forward;

    [FieldOffset(32)]
    public Vector3D<float> Right;

    [FieldOffset(48)]
    public Vector3D<float> Up;

    [FieldOffset(60)]
    public float Fov;
}
