using Silk.NET.Maths;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Interfaces;

public interface ISurface
{
    SurfaceType SurfaceType { get; }

    nint[] Handles { get; }

    Vector2D<uint> GetSize();
}
