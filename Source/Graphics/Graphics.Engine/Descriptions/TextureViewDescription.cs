using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct TextureViewDescription(Texture target,
                                     CubeMapFace baseFace,
                                     uint faceCount,
                                     uint baseMipLevel,
                                     uint mipLevels)
{
    /// <summary>
    /// The desired target.
    /// </summary>
    public Texture Target { get; set; } = target;

    /// <summary>
    /// If it is a cube map, it indicates the starting face to view. (Cube Map exclusive)
    /// </summary>
    public CubeMapFace BaseFace { get; set; } = baseFace;

    /// <summary>
    /// Number of faces to view. (Cube Map exclusive)
    /// </summary>
    public uint FaceCount { get; set; } = faceCount;

    /// <summary>
    /// The base mip level visible in the view. Must be less than the number of mip levels in the target Texture.
    /// </summary>
    public uint BaseMipLevel { get; set; } = baseMipLevel;

    /// <summary>
    /// The number of mip levels visible in the view.
    /// </summary>
    public uint MipLevels { get; set; } = mipLevels;

    public static TextureViewDescription Create(Texture target)
    {
        return new TextureViewDescription(target,
                                          CubeMapFace.PositiveX,
                                          1,
                                          0,
                                          target.Description.MipLevels);
    }
}
