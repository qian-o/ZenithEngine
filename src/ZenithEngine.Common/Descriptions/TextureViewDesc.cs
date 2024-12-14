using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct TextureViewDesc
{
    /// <summary>
    /// The desired target.
    /// </summary>
    public Texture Target;

    /// <summary>
    /// The format of the view.
    /// </summary>
    public PixelFormat Format;

    /// <summary>
    /// The base mip level visible in the view. Must be less than the number of mip levels in the target Texture.
    /// </summary>
    public uint BaseMipLevel;

    /// <summary>
    /// The number of mip levels visible in the view.
    /// </summary>
    public uint MipLevels;

    /// <summary>
    /// If it is a cube map, it indicates the starting face to view. (Cube Map exclusive)
    /// </summary>
    public CubeMapFace BaseFace;

    /// <summary>
    /// Number of faces to view. (Cube Map exclusive)
    /// </summary>
    public uint FaceCount;

    public static TextureViewDesc Default(Texture target,
                                          PixelFormat? format = null,
                                          uint baseMipLevel = 0,
                                          uint mipLevels = 1,
                                          CubeMapFace baseFace = CubeMapFace.PositiveX,
                                          uint faceCount = 1)
    {
        format ??= target.Desc.Format;

        return new()
        {
            Target = target,
            Format = format.Value,
            BaseMipLevel = baseMipLevel,
            MipLevels = mipLevels,
            BaseFace = baseFace,
            FaceCount = faceCount
        };
    }
}
