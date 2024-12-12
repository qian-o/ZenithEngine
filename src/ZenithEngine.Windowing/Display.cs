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
    public readonly int Index = index;

    public readonly string Name = name;

    public readonly Vector2D<int> MainPosition = mainPosition;

    public readonly Vector2D<uint> MainSize = mainSize;

    public readonly Vector2D<int> WorkPosition = workPosition;

    public readonly Vector2D<uint> WorkSize = workSize;

    public readonly float DpiScale = dpiScale;
}