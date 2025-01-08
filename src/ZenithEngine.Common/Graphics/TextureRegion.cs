using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public struct TextureRegion
{
    public uint Width;

    public uint Height;

    public uint Depth;

    public uint X;

    public uint Y;

    public uint Z;

    public uint MipLevel;

    public CubeMapFace Face;

    public static TextureRegion Default(uint width,
                                        uint height,
                                        uint depth,
                                        uint x = 0,
                                        uint y = 0,
                                        uint z = 0,
                                        uint mipLevel = 0,
                                        CubeMapFace face = CubeMapFace.PositiveX)
    {
        return new()
        {
            Width = width,
            Height = height,
            Depth = depth,
            X = x,
            Y = y,
            Z = z,
            MipLevel = mipLevel,
            Face = face
        };
    }
}
