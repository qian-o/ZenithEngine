using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct GraphicsShaderDesc
{
    public Shader? Vertex;

    public Shader? Hull;

    public Shader? Domain;

    public Shader? Geometry;

    public Shader? Pixel;

    public static GraphicsShaderDesc New(Shader? vertex = null,
                                         Shader? hull = null,
                                         Shader? domain = null,
                                         Shader? geometry = null,
                                         Shader? pixel = null)
    {
        return new()
        {
            Vertex = vertex,
            Hull = hull,
            Domain = domain,
            Geometry = geometry,
            Pixel = pixel
        };
    }
}
