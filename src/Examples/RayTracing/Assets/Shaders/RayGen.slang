import "Common/Math";
import "Common/Random";
import "Common/Structs";
import "Common/Uniforms";

[shader("raygeneration")]
void Main()
{
    uint2 launchID = DispatchRaysIndex().xy;
    uint2 launchSize = DispatchRaysDimensions().xy;

    uint seed = Random.Tea(launchSize.x * launchID.y + launchID.x, globals.FrameIndex);

    float3 radiance = float3(0.0, 0.0, 0.0);

    for (uint i = 0; i < globals.SampleCount; i++)
    {
        RayDesc rayDesc = globals.Camera.GetRayDesc(float2(Random.Rnd(seed), Random.Rnd(seed)));

        Payload payload;
        for (uint j = 0; j < globals.MaxDepth; j++)
        {
            TraceRay(scene, RAY_FLAG_FORCE_OPAQUE, 0xFF, 0, 0, 0, rayDesc, payload);

            if (payload.Stop)
            {
                break;
            }
        }

        radiance += payload.Radiance;
    }

    radiance /= float(globals.SampleCount);

    // HDR scale
    float4 accumulated = globals.FrameIndex == 0 ? float4(0.0, 0.0, 0.0, 0.0) : accumulation[launchID];
    float3 accumulatedRadiance = accumulated.rgb * radiance;

    accumulation[launchID] = float4(accumulatedRadiance, 1.0);

    float inv = 1.0 / float(globals.FrameIndex);
    radiance = accumulatedRadiance * inv;

    // LDR scale
    float3 ldr = Math.ToneMap(radiance, 1.5);

    output[launchID] = float4(ldr, 1.0);
}
