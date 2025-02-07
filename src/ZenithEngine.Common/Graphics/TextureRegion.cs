using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public struct TextureRegion(uint x = 0,
                            uint y = 0,
                            uint z = 0,
                            uint mipLevel = 0,
                            uint arrayLayer = 0,
                            CubeMapFace face = CubeMapFace.PositiveX,
                            uint width = 0,
                            uint height = 0,
                            uint depth = 0)
{
    public TexturePosition Position = new(x, y, z, mipLevel, arrayLayer, face);

    public uint Width = width;

    public uint Height = height;

    public uint Depth = depth;
}
