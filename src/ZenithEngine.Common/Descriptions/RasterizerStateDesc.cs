using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct RasterizerStateDesc(CullMode cullMode = CullMode.Back,
                                  FillMode fillMode = FillMode.Solid,
                                  FrontFace frontFace = FrontFace.CounterClockwise,
                                  int depthBias = 0,
                                  float depthBiasClamp = 0,
                                  float slopeScaledDepthBias = 0,
                                  bool depthClipEnabled = true,
                                  bool scissorEnabled = false)
{
    public RasterizerStateDesc()
    {
    }

    /// <summary>
    /// Controls which face will be culled.
    /// </summary>
    public CullMode CullMode = cullMode;

    /// <summary>
    /// Controls how the rasterizer fills polygons.
    /// </summary>
    public FillMode FillMode = fillMode;

    /// <summary>
    /// Controls the winding order used to determine the front face of primitives.
    /// </summary>
    public FrontFace FrontFace = frontFace;

    /// <summary>
    /// Depth value added to a given pixel. For info about depth bias.
    /// </summary>
    public int DepthBias = depthBias;

    /// <summary>
    /// Maximum depth bias of a pixel.
    /// </summary>
    public float DepthBiasClamp = depthBiasClamp;

    /// <summary>
    /// Scalar on a given pixel's slope.
    /// </summary>
    public float SlopeScaledDepthBias = slopeScaledDepthBias;

    /// <summary>
    /// Controls whether depth clipping is enabled.
    /// </summary>
    public bool DepthClipEnabled = depthClipEnabled;

    /// <summary>
    /// Controls whether the scissor test is enabled.
    /// </summary>
    public bool ScissorEnabled = scissorEnabled;
}
