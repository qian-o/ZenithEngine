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
    public uint X = x;

    public uint Y = y;

    public uint Z = z;

    public uint Width = width;

    public uint Height = height;

    public uint Depth = depth;

    public uint MipLevel = mipLevel;

    public CubeMapFace Face = face;

    public readonly bool SizeEquals(TextureRegion other)
    {
        return Width == other.Width && Height == other.Height && Depth == other.Depth;
    }
}
