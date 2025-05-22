import "Random";
import "Structs";

public class HDR
{
    public static float EnvPdf(in Uniforms uniforms)
    {
        Texture2D hdr1 = uniforms.HdrTextures[1];
        Texture2D hdr2 = uniforms.HdrTextures[2];
        SamplerState sampler1 = uniforms.HdrSamplers[1];
        SamplerState sampler2 = uniforms.HdrSamplers[2];

        float3 dir = WorldRayDirection();

        float theta = acos(clamp(dir.y, -1.0, 1.0));
        float2 uv = float2((PI + atan2(dir.z, dir.x)) * INV_2PI, theta * INV_PI);

        float pdf = hdr1.SampleLevel(sampler1, uv, 0).y * hdr2.SampleLevel(sampler2, float2(0.0, uv.y), 0).y;

        return (pdf * uniforms.Globals.HdrResolution) / (TWO_PI * PI * sin(theta));
    }

    public static float4 EnvSample(in Uniforms uniforms, inout uint seed, out float3 color)
    {
        Texture2D hdr0 = uniforms.HdrTextures[0];
        Texture2D hdr1 = uniforms.HdrTextures[1];
        Texture2D hdr2 = uniforms.HdrTextures[2];
        SamplerState sampler0 = uniforms.HdrSamplers[0];
        SamplerState sampler1 = uniforms.HdrSamplers[1];
        SamplerState sampler2 = uniforms.HdrSamplers[2];

        float r1 = Random.Rnd(seed);
        float r2 = Random.Rnd(seed);

        float v = hdr2.SampleLevel(sampler2, float2(0.0, r1), 0).x;
        float u = hdr1.SampleLevel(sampler1, float2(r2, v), 0).x;

        color = hdr0.SampleLevel(sampler0, float2(u, v), 0).xyz * uniforms.Globals.HdrMultiplier;
        float pdf = hdr1.SampleLevel(sampler1, float2(u, v), 0).y * hdr2.SampleLevel(sampler2, float2(0.0, v), 0).y;

        float phi = u * TWO_PI;
        float theta = v * PI;

        if (sin(theta) == 0.0)
        {
            pdf = 0.0;
        }

        return float4(-sin(theta) * cos(phi), cos(theta), -sin(theta) * sin(phi),
                      (pdf * uniforms.Globals.HdrResolution) / (TWO_PI * PI * sin(theta)));
    }
}
