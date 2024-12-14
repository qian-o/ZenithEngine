using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public struct TexturePosition(uint x = 0,
                              uint y = 0,
                              uint z = 0,
                              uint mipLevel = 0,
                              CubeMapFace face = CubeMapFace.PositiveX)
{
    public uint X = x;

    public uint Y = y;

    public uint Z = z;

    public uint MipLevel = mipLevel;

    public CubeMapFace Face = face;
}
