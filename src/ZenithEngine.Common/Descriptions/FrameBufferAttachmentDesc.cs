using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct FrameBufferAttachmentDesc(Texture target,
                                        uint mipLevel = 0,
                                        uint arrayLayer = 0,
                                        CubeMapFace face = CubeMapFace.PositiveX)
{
    public FrameBufferAttachmentDesc()
    {
    }

    /// <summary>
    /// The target texture to render into.
    /// </summary>
    public Texture Target = target;

    /// <summary>
    /// The mip level to render to.
    /// </summary>
    public uint MipLevel = mipLevel;

    /// <summary>
    /// The array layer to render to.
    /// </summary>
    public uint ArrayLayer = arrayLayer;

    /// <summary>
    /// If the target is a cube map, the face to render to. (Cube Map exclusive)
    /// </summary>
    public CubeMapFace Face = face;
}
