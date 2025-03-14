namespace Common.Helpers;

public static class MathExtensions
{
    public static float ToRadians(this float degrees)
    {
        return degrees * MathF.PI / 180.0f;
    }

    public static float ToDegrees(this float radians)
    {
        return radians * 180.0f / MathF.PI;
    }

    public static double ToRadians(this double degrees)
    {
        return degrees * Math.PI / 180.0f;
    }

    public static double ToDegrees(this double radians)
    {
        return radians * 180.0f / Math.PI;
    }
}
