using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public struct TextureRegion(uint x = 0,
                            uint y = 0,
                            uint z = 0,
                            uint width = 0,
                            uint height = 0,
                            uint depth = 0,
                            uint mipLevel = 0,
                            CubeMapFace face = CubeMapFace.PositiveX)
{
    public uint X { get; set; } = x;

    public uint Y { get; set; } = y;

    public uint Z { get; set; } = z;

    public uint Width { get; set; } = width;

    public uint Height { get; set; } = height;

    public uint Depth { get; set; } = depth;

    public uint MipLevel { get; set; } = mipLevel;

    public CubeMapFace Face { get; set; } = face;

    public readonly bool SizeEquals(TextureRegion other)
    {
        return Width == other.Width && Height == other.Height && Depth == other.Depth;
    }
}
