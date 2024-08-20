using System.Numerics;

namespace Graphics.Core;

public record struct Display
{
    public Display(int index,
                   string name,
                   Vector2 mainPosition,
                   Vector2 mainSize,
                   Vector2 workPosition,
                   Vector2 workSize,
                   float dpiScale)
    {
        Index = index;
        Name = name;
        MainPosition = mainPosition;
        MainSize = mainSize;
        WorkPosition = workPosition;
        WorkSize = workSize;
        DpiScale = dpiScale;
    }

    public int Index { get; set; }

    public string Name { get; set; }

    public Vector2 MainPosition { get; set; }

    public Vector2 MainSize { get; set; }

    public Vector2 WorkPosition { get; set; }

    public Vector2 WorkSize { get; set; }

    public float DpiScale { get; set; }
}