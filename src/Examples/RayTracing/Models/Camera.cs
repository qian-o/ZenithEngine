using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace RayTracing.Models;

[StructLayout(LayoutKind.Explicit)]
internal struct Camera
{
    [FieldOffset(0)]
    public Vector3D<float> Position;

    [FieldOffset(12)]
    public Vector3D<float> Forward;

    [FieldOffset(24)]
    public Vector3D<float> Right;

    [FieldOffset(36)]
    public Vector3D<float> Up;

    [FieldOffset(48)]
    public float Fov;
}
