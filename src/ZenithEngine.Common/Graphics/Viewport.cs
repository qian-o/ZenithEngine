namespace ZenithEngine.Common.Graphics;

public struct Viewport(float x = 0,
                       float y = 0,
                       float width = 0,
                       float height = 0,
                       float minDepth = 0,
                       float maxDepth = 1)
{
    public float X = x;

    public float Y = y;

    public float Width = width;

    public float Height = height;

    public float MinDepth = minDepth;

    public float MaxDepth = maxDepth;
}
