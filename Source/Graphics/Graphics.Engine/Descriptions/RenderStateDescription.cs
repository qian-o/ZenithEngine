namespace Graphics.Engine.Descriptions;

public struct RenderStateDescription
{
    /// <summary>
    /// Gets or sets the Rasterizer State.
    /// </summary>
    public RasterizerStateDescription RasterizerState { get; set; }

    /// <summary>
    /// Gets or sets the DepthStencil state.
    /// </summary>
    public DepthStencilStateDescription DepthStencilState { get; set; }
}
