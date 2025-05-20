import "Random";
import "Structs";
import "Uniforms";

public class HDR
{
    public static float EnvPdf()
    {
        float3 dir = WorldRayDirection();
        float theta = acos(clamp(dir.y, -1.0, 1.0));
        float2 uv = float2((PI + atan2(dir.z, dir.x)) * INV_2PI, theta * INV_PI);

        float pdf = hdrTextures[1].SampleLevel(hdrSamplers[1], uv, 0).y *
                    hdrTextures[2].SampleLevel(hdrSamplers[2], float2(0.0, uv.y), 0).y;

        return (pdf * globals.HdrResolution) / (TWO_PI * PI * sin(theta));
    }

    public static float4 EnvSample(inout float3 color, uint seed)
    {
        float r1 = Random.Rnd(seed);
        float r2 = Random.Rnd(seed);

        float v = hdrTextures[2].SampleLevel(hdrSamplers[2], float2(0.0, r1), 0).x;
        float u = hdrTextures[1].SampleLevel(hdrSamplers[1], float2(r2, v), 0).x;

        color = hdrTextures[0].SampleLevel(hdrSamplers[0], float2(u, v), 0).xyz * globals.HdrMultiplier;
        float pdf = hdrTextures[1].SampleLevel(hdrSamplers[1], float2(u, v), 0).y *
                    hdrTextures[2].SampleLevel(hdrSamplers[2], float2(0.0, v), 0).y;

        float phi = u * TWO_PI;
        float theta = v * PI;

        if (sin(theta) == 0.0)
        {
            pdf = 0.0;
        }

        return float4(-sin(theta) * cos(phi), cos(theta), -sin(theta) * sin(phi),
                      (pdf * globals.HdrResolution) / (TWO_PI * PI * sin(theta)));
    }
}
