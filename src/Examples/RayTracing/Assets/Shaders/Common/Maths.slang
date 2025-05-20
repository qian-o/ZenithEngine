public class Maths
{
    public static float3 ToneMap(float3 rgb, float limit)
    {
        float luminance = 0.299 * rgb.x + 0.587 * rgb.y + 0.114 * rgb.z;

        return rgb / (1.0 + luminance / limit);
    }

    public static float PowerHeuristic(float a, float b)
    {
        float t = a * a;

        return t / (t + b * b);
    }
}
