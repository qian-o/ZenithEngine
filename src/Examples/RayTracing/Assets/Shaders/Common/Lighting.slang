import "HDR";
import "Structs";
import "Uniforms";

public class Lighting
{
    public static float3 DirectLighting(inout Payload payload, in Material material)
    {
        float3 L = float3(0.0, 0.0, 0.0);

        BSDFSample bsdfSample;

        float3 surfacePos = payload.Position;

        if (globals.UseHdr)
        {
            float3 color = float3(0.0, 0.0, 0.0);
            float4 dirPdf = HDR.EnvSample(color, payload.Seed);

            float3 lightDir = dirPdf.xyz;
            float pdf = dirPdf.w;

            ShadowPayload shadowPayload = {};

            RayDesc rayDesc;
            rayDesc.Origin = surfacePos;
            rayDesc.Direction = lightDir;
            rayDesc.TMin = NEAR;
            rayDesc.TMax = 1;

            TraceRay(scene,
                     RAY_FLAG_FORCE_OPAQUE | RAY_FLAG_ACCEPT_FIRST_HIT_AND_END_SEARCH |
                         RAY_FLAG_SKIP_CLOSEST_HIT_SHADER,
                     0xFF, 0, 0, 1, rayDesc, shadowPayload);

            if (!shadowPayload.InShadow)
            {
            }
        }
    }
}
