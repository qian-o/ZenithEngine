import "Constants";
import "Random";

public class Maths
{
    public static float Max3(float3 v)
    {
        return max(max(v.x, v.y), v.z);
    }

    public static float3 ToneMap(float3 rgb, float limit)
    {
        float luminance = 0.299 * rgb.x + 0.587 * rgb.y + 0.114 * rgb.z;

        return rgb / (1.0 + luminance / limit);
    }

    public static float3x3 LocalFrame(float3 normal)
    {
        float3 up = abs(normal.z) < 0.999 ? float3(0, 0, 1) : float3(1, 0, 0);
        float3 tangentX = normalize(cross(up, normal));
        float3 tangentY = cross(normal, tangentX);
        return float3x3(tangentX, tangentY, normal);
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

    public static float GTR1(float NDotH, float a)
    {
        if (a >= 1.0)
        {
            return (1.0 / Constants.PI);
        }

        float a2 = a * a;
        float t = 1.0 + (a2 - 1.0) * NDotH * NDotH;

        return (a2 - 1.0) / (Constants.PI * log(a2) * t);
    }

    public static float GTR2(float NDotH, float a)
    {
        float a2 = a * a;
        float t = 1.0 + (a2 - 1.0) * NDotH * NDotH;
        return a2 / (Constants.PI * t * t);
    }

    public static float3 ImportanceSampleGTR1(float rgh, float r1, float r2)
    {
        float a = max(0.001, rgh);
        float a2 = a * a;

        float phi = r1 * Constants.TWO_PI;

        float cosTheta = sqrt((1.0 - pow(a2, 1.0 - r1)) / (1.0 - a2));
        float sinTheta = clamp(sqrt(1.0 - (cosTheta * cosTheta)), 0.0, 1.0);
        float sinPhi = sin(phi);
        float cosPhi = cos(phi);

        return float3(sinTheta * cosPhi, sinTheta * sinPhi, cosTheta);
    }

    public static float3 ImportanceSampleGTR2_aniso(float ax, float ay, float r1, float r2)
    {
        float phi = r1 * Constants.TWO_PI;

        float sinPhi = ay * sin(phi);
        float cosPhi = ax * cos(phi);
        float tanTheta = sqrt(r2 / (1 - r2));

        return float3(tanTheta * cosPhi, tanTheta * sinPhi, 1.0);
    }

    public static float3 ImportanceSampleGTR2(float rgh, float r1, float r2)
    {
        float a = max(0.001, rgh);

        float phi = r1 * Constants.TWO_PI;

        float cosTheta = sqrt((1.0 - r2) / (1.0 + (a * a - 1.0) * r2));
        float sinTheta = clamp(sqrt(1.0 - (cosTheta * cosTheta)), 0.0, 1.0);
        float sinPhi = sin(phi);
        float cosPhi = cos(phi);

        return float3(sinTheta * cosPhi, sinTheta * sinPhi, cosTheta);
    }

    public static float SmithGGX(float NDotv, float alphaG)
    {
        float a = alphaG * alphaG;
        float b = NDotv * NDotv;
        return 1.0 / (NDotv + sqrt(a + b - a * b));
    }

    public static float SchlickFresnel(float u)
    {
        float m = clamp(1.0 - u, 0.0, 1.0);
        return m * m * m * m * m; // power of 5
    }

    public static float3 UniformSampleSphere(inout uint seed)
    {
        float r1 = Random.Rnd(seed);
        float r2 = Random.Rnd(seed);

        float z = 1.0 - 2.0 * r1;
        float r = sqrt(max(0.0, 1.0 - z * z));
        float phi = 2.0 * Constants.PI * r2;

        float x = r * cos(phi);
        float y = r * sin(phi);

        return float3(x, y, z);
    }

    public static float3 CosineSampleHemisphere(inout uint seed)
    {
        float r1 = Random.Rnd(seed);
        float r2 = Random.Rnd(seed);

        float3 dir;
        float r = sqrt(r1);
        float phi = 2.0 * Constants.PI * r2;

        dir.x = r * cos(phi);
        dir.y = r * sin(phi);
        dir.z = sqrt(max(0.0, 1.0 - dir.x * dir.x - dir.y * dir.y));

        return dir;
    }
}
