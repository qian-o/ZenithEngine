using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct RasterizerStateDesc
{
    /// <summary>
    /// Controls which face will be culled.
    /// </summary>
    public CullMode CullMode;

    /// <summary>
    /// Controls how the rasterizer fills polygons.
    /// </summary>
    public FillMode FillMode;

    /// <summary>
    /// Controls the winding order used to determine the front face of primitives.
    /// </summary>
    public FrontFace FrontFace;

    /// <summary>
    /// Depth value added to a given pixel. For info about depth bias.
    /// </summary>
    public int DepthBias;

    /// <summary>
    /// Maximum depth bias of a pixel.
    /// </summary>
    public float DepthBiasClamp;

    /// <summary>
    /// Scalar on a given pixel's slope.
    /// </summary>
    public float SlopeScaledDepthBias;

    /// <summary>
    /// Controls whether depth clipping is enabled.
    /// </summary>
    public bool DepthClipEnabled;

    /// <summary>
    /// Controls whether the scissor test is enabled.
    /// </summary>
    public bool ScissorEnabled;

    public static RasterizerStateDesc New(CullMode cullMode = CullMode.Back,
                                          FillMode fillMode = FillMode.Solid,
                                          FrontFace frontFace = FrontFace.CounterClockwise,
                                          int depthBias = 0,
                                          float depthBiasClamp = 0,
                                          float slopeScaledDepthBias = 0,
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
