namespace ZenithEngine.Common.Graphics;

public struct Viewport(float x = 0,
                       float y = 0,
                       float width = 0,
                       float height = 0,
                       float minDepth = 0,
                       float maxDepth = 1)
{
    public float X { get; set; } = x;

    public float Y { get; set; } = y;

    public float Width { get; set; } = width;

    public float Height { get; set; } = height;

    public float MinDepth { get; set; } = minDepth;

    public float MaxDepth { get; set; } = maxDepth;
}
