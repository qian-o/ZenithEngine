namespace Graphics.Core;

public struct RgbaFloat(float r, float g, float b, float a) : IEquatable<RgbaFloat>
{
    public static readonly RgbaFloat Red = new(1.0f, 0.0f, 0.0f, 1.0f);

    public static readonly RgbaFloat DarkRed = new(0.6f, 0.0f, 0.0f, 1.0f);

    public static readonly RgbaFloat Green = new(0.0f, 1.0f, 0.0f, 1.0f);

    public static readonly RgbaFloat Blue = new(0.0f, 0.0f, 1.0f, 1.0f);

    public static readonly RgbaFloat Yellow = new(1.0f, 1.0f, 0.0f, 1.0f);

    public static readonly RgbaFloat Grey = new(0.25f, 0.25f, 0.25f, 1.0f);

    public static readonly RgbaFloat LightGrey = new(0.65f, 0.65f, 0.65f, 1.0f);

    public static readonly RgbaFloat Cyan = new(0.0f, 1.0f, 1.0f, 1.0f);

    public static readonly RgbaFloat White = new(1.0f, 1.0f, 1.0f, 1.0f);

    public static readonly RgbaFloat CornflowerBlue = new(0.3921f, 0.5843f, 0.9294f, 1.0f);

    public static readonly RgbaFloat Clear = new(0.0f, 0.0f, 0.0f, 0.0f);

    public static readonly RgbaFloat Black = new(0.0f, 0.0f, 0.0f, 1.0f);

    public static readonly RgbaFloat Pink = new(1.0f, 0.45f, 0.75f, 1.0f);

    public static readonly RgbaFloat Orange = new(1.0f, 0.36f, 0.0f, 1.0f);

    public float R { get; set; } = r;

    public float G { get; set; } = g;

    public float B { get; set; } = b;

    public float A { get; set; } = a;

    public readonly bool Equals(RgbaFloat other)
    {
        return R == other.R
               && G == other.G
               && B == other.B
               && A == other.A;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(R.GetHashCode(),
                                  G.GetHashCode(),
                                  B.GetHashCode(),
                                  A.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is RgbaFloat rgbaFloat && Equals(rgbaFloat);
    }

    public override readonly string ToString()
    {
        return $"R: {R}, G: {G}, B: {B}, A: {A}";
    }

    public static bool operator ==(RgbaFloat left, RgbaFloat right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RgbaFloat left, RgbaFloat right)
    {
        return !(left == right);
    }
}
