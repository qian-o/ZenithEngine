using System.Numerics;

namespace Graphics.Core;

public readonly record struct Display
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

    public int Index { get; init; }

    public string Name { get; init; }

    public Vector2 MainPosition { get; init; }

    public Vector2 MainSize { get; init; }

    public Vector2 WorkPosition { get; init; }

    public Vector2 WorkSize { get; init; }

    public float DpiScale { get; init; }
}