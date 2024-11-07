﻿using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct TextureViewDescription
{
    /// <summary>
    /// The desired target.
    /// </summary>
    public Texture Target { get; set; }

    /// <summary>
    /// If it is a cube map, it indicates the starting face to view. (Cube Map exclusive)
    /// </summary>
    public CubeMapFace BaseFace { get; set; }

    /// <summary>
    /// Number of faces to view. (Cube Map exclusive)
    /// </summary>
    public uint FaceCount { get; set; }

    /// <summary>
    /// The base mip level visible in the view. Must be less than the number of mip levels in the target Texture.
    /// </summary>
    public uint BaseMipLevel { get; set; }

    /// <summary>
    /// The number of mip levels visible in the view.
    /// </summary>
    public uint MipLevels { get; set; }

    public static TextureViewDescription Default(Texture target)
    {
        return new()
        {
            Target = target,
            BaseFace = CubeMapFace.PositiveX,
            FaceCount = 6,
            BaseMipLevel = 0,
            MipLevels = target.Description.MipLevels
        };
    }
}
