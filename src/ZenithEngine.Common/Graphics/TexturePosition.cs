using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public struct TexturePosition(uint x = 0,
                              uint y = 0,
                              uint z = 0,
                              uint mipLevel = 0,
                              CubeMapFace face = CubeMapFace.PositiveX)
{
    public uint X { get; set; } = x;

    public uint Y { get; set; } = y;

    public uint Z { get; set; } = z;

    public uint MipLevel { get; set; } = mipLevel;

    public CubeMapFace Face { get; set; } = face;
}
