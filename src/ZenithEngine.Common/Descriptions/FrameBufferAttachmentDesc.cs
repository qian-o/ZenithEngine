using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct FrameBufferAttachmentDesc
{
    /// <summary>
    /// The target texture to render into.
    /// </summary>
    public Texture Target;

    /// <summary>
    /// The mip level to render to.
    /// </summary>
    public uint MipLevel;

    /// <summary>
    /// The array layer to render to.
    /// </summary>
    public uint ArrayLayer;

    /// <summary>
    /// If the target is a cube map, the face to render to. (Cube Map exclusive)
    /// </summary>
    public CubeMapFace Face;

    public static FrameBufferAttachmentDesc Default(Texture target,
                                                    uint mipLevel = 0,
                                                    uint arrayLayer = 0,
                                                    CubeMapFace face = CubeMapFace.PositiveX)
    {
        return new()
        {
            Target = target,
            MipLevel = mipLevel,
            ArrayLayer = arrayLayer,
            Face = face
        };
    }
}
