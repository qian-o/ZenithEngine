using System.Runtime.InteropServices;

namespace RayTracing.Models;

[StructLayout(LayoutKind.Explicit)]
internal struct Globals
{
    [FieldOffset(0)]
    public Camera Camera;

    [FieldOffset(64)]
    public bool DoubleSidedLighting;

    [FieldOffset(68)]
    public uint SampleCount;

    [FieldOffset(72)]
    public uint MaxDepth;

    [FieldOffset(76)]
    public uint FrameIndex;

    [FieldOffset(80)]
    public bool UseHdr;

    [FieldOffset(84)]
    public float HdrMultiplier;

    [FieldOffset(88)]
    public float HdrResolution;
}