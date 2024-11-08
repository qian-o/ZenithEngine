namespace Graphics.Engine.Descriptions;

public struct GraphicsShaderDesc
{
    public LayoutDesc[] InputLayout { get; set; }

    public Shader? Vertex { get; set; }

    public Shader? Hull { get; set; }

    public Shader? Domain { get; set; }

    public Shader? Geometry { get; set; }

    public Shader? Pixel { get; set; }

    public static GraphicsShaderDesc Default(LayoutDesc[] inputLayout,
                                             Shader? vertex = null,
                                             Shader? hull = null,
                                             Shader? domain = null,
                                             Shader? geometry = null,
                                             Shader? pixel = null)
    {
        return new GraphicsShaderDesc
        {
            InputLayout = inputLayout,
            Vertex = vertex,
            Hull = hull,
            Domain = domain,
            Geometry = geometry,
            Pixel = pixel
        };
    }
}
