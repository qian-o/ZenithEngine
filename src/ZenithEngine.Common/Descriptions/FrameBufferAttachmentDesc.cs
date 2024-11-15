using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct FrameBufferAttachmentDesc
{
    /// <summary>
    /// The target texture to render into.
    /// </summary>
    public Texture Target { get; set; }

    /// <summary>
    /// The mip level to render to.
    /// </summary>
    public uint MipLevel { get; set; }

    /// <summary>
    /// If the target is a cube map, the face to render to. (Cube Map exclusive)
    /// </summary>
    public CubeMapFace Face { get; set; }

    public static FrameBufferAttachmentDesc Default(Texture target,
                                                    uint mipLevel = 0,
                                                    CubeMapFace face = CubeMapFace.PositiveX)
    {
        return new()
        {
            Target = target,
            MipLevel = mipLevel,
            Face = face
        };
    }
}
