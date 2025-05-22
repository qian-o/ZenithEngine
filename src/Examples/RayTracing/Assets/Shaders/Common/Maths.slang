import "Constants";

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

    public static float DielectricFresnel(float cos_theta_i, float eta)
    {
        float sinThetaTSq = eta * eta * (1.0f - cos_theta_i * cos_theta_i);

        // Total internal reflection
        if (sinThetaTSq > 1.0)
        {
            return 1.0;
        }

        float cos_theta_t = sqrt(max(1.0 - sinThetaTSq, 0.0));

        float rs = (eta * cos_theta_t - cos_theta_i) / (eta * cos_theta_t + cos_theta_i);
        float rp = (eta * cos_theta_i - cos_theta_t) / (eta * cos_theta_i + cos_theta_t);

        return 0.5f * (rs * rs + rp * rp);
    }

    public static float GTR2(float NDotH, float a)
    {
        float a2 = a * a;
        float t = 1.0 + (a2 - 1.0) * NDotH * NDotH;
        return a2 / (Constants.PI * t * t);
    }

    public static float SmithGGX(float NDotv, float alphaG)
    {
        float a = alphaG * alphaG;
        float b = NDotv * NDotv;
        return 1.0 / (NDotv + sqrt(a + b - a * b));
    }
}
