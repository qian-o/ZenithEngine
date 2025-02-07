using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public struct TexturePosition
{
    public uint X;

    public uint Y;

    public uint Z;

    public uint MipLevel;

    public uint ArrayLayer;

    public CubeMapFace Face;

    public static TexturePosition New(uint x = 0,
                                      uint y = 0,
                                      uint z = 0,
                                      uint mipLevel = 0,
                                      uint arrayLayer = 0,
                                      CubeMapFace face = CubeMapFace.PositiveX)
    {
        return new()
        {
            X = x,
            Y = y,
            Z = z,
            MipLevel = mipLevel,
            ArrayLayer = arrayLayer,
            Face = face
        };
    }
}
