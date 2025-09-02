using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct MeshShaderDesc(Shader? amplification = null,
                             Shader? mesh = null,
                             Shader? pixel = null)
{
    public MeshShaderDesc() : this(null, null, null)
    {
    }

    public Shader? Amplification = amplification;

    public Shader? Mesh = mesh;

    public Shader? Pixel = pixel;
}
