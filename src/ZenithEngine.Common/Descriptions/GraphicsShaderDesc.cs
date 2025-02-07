using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct GraphicsShaderDesc(Shader? vertex = null,
                                 Shader? hull = null,
                                 Shader? domain = null,
                                 Shader? geometry = null,
                                 Shader? pixel = null)
{
    public Shader? Vertex = vertex;

    public Shader? Hull = hull;

    public Shader? Domain = domain;

    public Shader? Geometry = geometry;

    public Shader? Pixel = pixel;
}
