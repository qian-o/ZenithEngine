using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct FrameBufferAttachmentDescription(Texture target, CubeMapFace face, uint mipLevel)
{
    /// <summary>
    /// The target texture to render into.
    /// </summary>
    public Texture Target { get; set; } = target;

    /// <summary>
    /// If the target is a cube map, the face to render to. (Cube Map exclusive)
    /// </summary>
    public CubeMapFace Face { get; set; } = face;

    /// <summary>
    /// The mip level to render to.
    /// </summary>
    public uint MipLevel { get; set; } = mipLevel;

    public static FrameBufferAttachmentDescription Create(Texture target)
    {
        return new FrameBufferAttachmentDescription(target, CubeMapFace.PositiveX, 0);
    }
}
