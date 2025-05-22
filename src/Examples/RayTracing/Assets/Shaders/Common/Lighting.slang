import "Constants";
import "Common/DisneyBSDF";
import "HDR";
import "Maths";
import "Random";
import "Sampling";
import "Structs";

public class Lighting
{
    public static float3 DirectLighting(in Uniforms uniforms, in Material material, inout Payload payload)
    {
        float3 L = float3(0.0, 0.0, 0.0);

        BSDFSample bsdfSample;

        float3 surfacePos = payload.Position;

        if (uniforms.Globals.UseHdr)
        {
            float3 color = float3(0.0, 0.0, 0.0);
            float4 dirPdf = HDR.EnvSample(uniforms, payload.Seed, color);

            float3 lightDir = dirPdf.xyz;
            float lightPdf = dirPdf.w;

            ShadowPayload shadowPayload = {};

            RayDesc rayDesc;
            rayDesc.Origin = surfacePos;
            rayDesc.TMin = Constants.MINIMUM;
            rayDesc.Direction = lightDir;
            rayDesc.TMax = Constants.INFINITY;

            TraceRay(uniforms.Scene,
                     RAY_FLAG_FORCE_OPAQUE | RAY_FLAG_ACCEPT_FIRST_HIT_AND_END_SEARCH |
                         RAY_FLAG_SKIP_CLOSEST_HIT_SHADER,
                     0xFF, 0, 0, 1, rayDesc, shadowPayload);

            if (!shadowPayload.InShadow)
            {
                float3 F = DisneyBSDF.Eval(payload, material, lightDir, bsdfSample.Pdf);

                float cosTheta = abs(dot(lightDir, payload.ForwardNormal));
                float misWeight = Maths.PowerHeuristic(lightPdf, bsdfSample.Pdf);

                if (misWeight > 0.0)
                {
                    L += misWeight * F * cosTheta * color / (lightPdf + Constants.EPS);
                }
            }
        }

        if (uniforms.Lights.getCount() > 0)
        {
            Light light = uniforms.Lights[int(Random.Rnd(payload.Seed) * float(uniforms.Lights.getCount()))];

            if (light.Type == LightType.Unknown)
            {
                return L;
            }

            LightSample sampled = Sampling.SampleLight(uniforms, light, payload.Seed);
            float3 lightDir = sampled.Position - surfacePos;
            float lightDist = length(lightDir);
            float lightDistSq = lightDist * lightDist;
            lightDir = normalize(lightDir);

            if (dot(payload.ForwardNormal, lightDir) <= 0.0 || dot(lightDir, sampled.Normal) >= 0.0)
            {
                return L;
            }

            ShadowPayload shadowPayload = {};

            RayDesc rayDesc;
            rayDesc.Origin = surfacePos;
            rayDesc.TMin = Constants.MINIMUM;
            rayDesc.Direction = lightDir;
            rayDesc.TMax = lightDist;

            TraceRay(uniforms.Scene,
                     RAY_FLAG_FORCE_OPAQUE | RAY_FLAG_ACCEPT_FIRST_HIT_AND_END_SEARCH |
                         RAY_FLAG_SKIP_CLOSEST_HIT_SHADER,
                     0xFF, 0, 0, 1, rayDesc, shadowPayload);

            if (!shadowPayload.InShadow)
            {
                float3 F = DisneyBSDF.Eval(payload, material, lightDir, bsdfSample.Pdf);

                float lightPdf = lightDistSq / (light.Area * abs(dot(sampled.Normal, lightDir)));
                float cosTheta = abs(dot(payload.ForwardNormal, lightDir));
                float misWeight = Maths.PowerHeuristic(lightPdf, bsdfSample.Pdf);

                L += misWeight * F * cosTheta * sampled.Emission / (lightPdf + Constants.EPS);
            }
        }

        return L;
    }
}
