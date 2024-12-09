using Silk.NET.Maths;

namespace ZenithEngine.Windowing;

public readonly struct Display(int index,
                               string name,
                               Vector2D<int> mainPosition,
                               Vector2D<int> mainSize,
                               Vector2D<int> workPosition,
                               Vector2D<int> workSize,
                               float dpiScale)
{
    public int Index { get; } = index;

    public string Name { get; } = name;

    public Vector2D<int> MainPosition { get; } = mainPosition;

    public Vector2D<int> MainSize { get; } = mainSize;

    public Vector2D<int> WorkPosition { get; } = workPosition;

    public Vector2D<int> WorkSize { get; } = workSize;

    public float DpiScale { get; } = dpiScale;
}