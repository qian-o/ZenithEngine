using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public struct TexturePosition
{
    public uint X;

    public uint Y;

    public uint Z;

    public uint MipLevel;

    public uint LayerIndex;

    public CubeMapFace Face;

    public static TexturePosition Default(uint x = 0,
                                          uint y = 0,
                                          uint z = 0,
                                          uint mipLevel = 0,
                                          uint layerIndex = 0,
                                          CubeMapFace face = CubeMapFace.PositiveX)
    {
        return new()
        {
            X = x,
            Y = y,
            Z = z,
            MipLevel = mipLevel,
            LayerIndex = layerIndex,
            Face = face
        };
    }
}
