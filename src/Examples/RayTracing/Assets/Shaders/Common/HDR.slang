import "Common/Random";
import "Common/Structs";

public class HDR
{
    public static float EnvPdf(float hdrResolution, Texture2D[] hdrs, SamplerState[] hdrSamplers)
    {
        float3 dir = WorldRayDirection();
        float theta = acos(clamp(dir.y, -1.0, 1.0));
        float2 uv = float2((PI + atan2(dir.z, dir.x)) * INV_2PI, theta * INV_PI);

        float pdf = hdrs[1].Sample(hdrSamplers[1], uv).y * hdrs[2].Sample(hdrSamplers[2], float2(0.0, uv.y)).y;

        return (pdf * hdrResolution) / (TWO_PI * PI * sin(theta));
    }

    public static float4 EnvSample(inout float3 color, uint seed, float hdrMultiplier, float hdrResolution,
                                   Texture2D[] hdrs, SamplerState[] hdrSamplers)
    {
        float r1 = Random.Rnd(seed);
        float r2 = Random.Rnd(seed);

        float v = hdrs[2].Sample(hdrSamplers[2], float2(0.0, r1)).x;
        float u = hdrs[1].Sample(hdrSamplers[1], float2(r2, v)).x;

        color = hdrs[0].Sample(hdrSamplers[0], float2(u, v)).xyz * hdrMultiplier;
        float pdf = hdrs[1].Sample(hdrSamplers[1], float2(u, v)).y * hdrs[2].Sample(hdrSamplers[2], float2(0.0, v)).y;

        float phi = u * TWO_PI;
        float theta = v * PI;

        if (sin(theta) == 0.0)
        {
            pdf = 0.0;
        }

        return float4(-sin(theta) * cos(phi), cos(theta), -sin(theta) * sin(phi),
                      (pdf * hdrResolution) / (TWO_PI * PI * sin(theta)));
    }
}
