import "Common/Constants";
import "Common/Maths";
import "Common/Sampling";
import "Common/Structs";

uniform ParameterBlock<Uniforms> uniforms;

[shader("miss")]
void Miss(inout Payload payload)
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
}

[shader("miss")]
void ShadowMiss(inout ShadowPayload payload)
{
    payload.InShadow = false;
}
