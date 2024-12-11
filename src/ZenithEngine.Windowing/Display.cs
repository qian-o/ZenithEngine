using Silk.NET.Maths;

namespace ZenithEngine.Windowing;

public readonly struct Display(int index,
                               string name,
                               Vector2D<int> mainPosition,
                               Vector2D<uint> mainSize,
                               Vector2D<int> workPosition,
                               Vector2D<uint> workSize,
                               float dpiScale)
{
    public int Index { get; } = index;

    public string Name { get; } = name;

    public Vector2D<int> MainPosition { get; } = mainPosition;

    public Vector2D<uint> MainSize { get; } = mainSize;

    public Vector2D<int> WorkPosition { get; } = workPosition;

    public Vector2D<uint> WorkSize { get; } = workSize;

    public float DpiScale { get; } = dpiScale;
}