using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct RasterizerStateDesc
{
    /// <summary>
    /// Controls which face will be culled.
    /// </summary>
    public CullMode CullMode { get; set; }

    /// <summary>
    /// Controls how the rasterizer fills polygons.
    /// </summary>
    public FillMode FillMode { get; set; }

    /// <summary>
    /// Controls the winding order used to determine the front face of primitives.
    /// </summary>
    public FrontFace FrontFace { get; set; }

    /// <summary>
    /// Depth value added to a given pixel. For info about depth bias.
    /// </summary>
    public int DepthBias { get; set; }

    /// <summary>
    /// Maximum depth bias of a pixel.
    /// </summary>
    public float DepthBiasClamp { get; set; }

    /// <summary>
    /// Scalar on a given pixel's slope.
    /// </summary>
    public float SlopeScaledDepthBias { get; set; }

    /// <summary>
    /// Controls whether depth clipping is enabled.
    /// </summary>
    public bool DepthClipEnabled { get; set; }

    /// <summary>
    /// Controls whether the scissor test is enabled.
    /// </summary>
    public bool ScissorEnabled { get; set; }

    public static RasterizerStateDesc Default(CullMode cullMode = CullMode.Back,
                                              FillMode fillMode = FillMode.Solid,
                                              FrontFace frontFace = FrontFace.CounterClockwise,
                                              int depthBias = 0,
                                              float depthBiasClamp = 0.0f,
                                              float slopeScaledDepthBias = 0.0f,
                                              bool depthClipEnabled = true,
                                              bool scissorEnabled = false)
    {
        return new()
        {
            CullMode = cullMode,
            FillMode = fillMode,
            FrontFace = frontFace,
            DepthBias = depthBias,
            DepthBiasClamp = depthBiasClamp,
            SlopeScaledDepthBias = slopeScaledDepthBias,
            DepthClipEnabled = depthClipEnabled,
            ScissorEnabled = scissorEnabled
        };
    }
}
