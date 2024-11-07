using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct RasterizerStateDescription(CullMode cullMode,
                                         FillMode fillMode,
                                         FrontFace frontFace,
                                         int depthBias,
                                         float depthBiasClamp,
                                         float slopeScaledDepthBias,
                                         bool depthClipEnabled,
                                         bool scissorEnabled)
{
    /// <summary>
    /// Controls which face will be culled.
    /// </summary>
    public CullMode CullMode { get; set; } = cullMode;

    /// <summary>
    /// Controls how the rasterizer fills polygons.
    /// </summary>
    public FillMode FillMode { get; set; } = fillMode;

    /// <summary>
    /// Controls the winding order used to determine the front face of primitives.
    /// </summary>
    public FrontFace FrontFace { get; set; } = frontFace;

    /// <summary>
    /// Depth value added to a given pixel. For info about depth bias.
    /// </summary>
    public int DepthBias { get; set; } = depthBias;

    /// <summary>
    /// Maximum depth bias of a pixel.
    /// </summary>
    public float DepthBiasClamp { get; set; } = depthBiasClamp;

    /// <summary>
    /// Scalar on a given pixel's slope.
    /// </summary>
    public float SlopeScaledDepthBias { get; set; } = slopeScaledDepthBias;

    /// <summary>
    /// Controls whether depth clipping is enabled.
    /// </summary>
    public bool DepthClipEnabled { get; set; } = depthClipEnabled;

    /// <summary>
    /// Controls whether the scissor test is enabled.
    /// </summary>
    public bool ScissorEnabled { get; set; } = scissorEnabled;

    public static RasterizerStateDescription Create(bool depthClipEnabled = true, bool scissorTestEnabled = false)
    {
        return new RasterizerStateDescription(CullMode.Back,
                                              FillMode.Solid,
                                              FrontFace.CounterClockwise,
                                              0,
                                              0.0f,
                                              0.0f,
                                              depthClipEnabled,
                                              scissorTestEnabled);
    }
}
