import "Common/Constants";
import "Common/HDR";
import "Common/Maths";
import "Common/Sampling";
import "Common/Structs";

uniform ParameterBlock<Uniforms> uniforms;

[shader("miss")]
void Main(inout Payload payload)
{
    // Stop path tracing loop from rgen shader
    payload.Stop = true;
    payload.ForwardNormal = float3(0.0, 0.0, 0.0);
    payload.Position = float3(0.0, 0.0, 0.0);

    LightSample lightSample;

    if (Sampling.InteresetsEmitter(uniforms, Constants.INFINITY, lightSample))
    {
        float3 Le = Sampling.SampleEmitter(payload, lightSample, payload.Bsdf);

        payload.Radiance += Le * payload.Beta;

        return;
    }

    if (uniforms.Globals.UseHdr)
    {
        float misWeight = 1.0;

        float2 uv = float2((Constants.PI + atan2(WorldRayDirection().z, WorldRayDirection().x)) * Constants.INV_2PI,
                           acos(WorldRayDirection().y) * Constants.INV_PI);

        if (payload.Depth > 0)
        {
            float lightPdf = HDR.EnvPdf(uniforms);

            misWeight = Maths.PowerHeuristic(payload.Bsdf.Pdf, lightPdf);
        }

        payload.Radiance += misWeight * uniforms.HdrTextures[0].SampleLevel(uniforms.HdrSamplers[0], uv, 0).xyz *
                            payload.Beta * uniforms.Globals.HdrMultiplier;

        payload.Stop = true;
    }
}

[shader("miss")]
void ShadowMain(inout ShadowPayload payload)
{
    payload.InShadow = false;
}
