﻿import "Common/Maths";
import "Common/Random";
import "Common/Structs";

uniform ParameterBlock<Uniforms> uniforms;

[shader("raygeneration")]
void RayGeneration()
{
    uint2 launchID = DispatchRaysIndex().xy;
    uint2 launchSize = DispatchRaysDimensions().xy;

    uint seed = Random.Tea(launchSize.x * launchID.y + launchID.x, uniforms.Globals.FrameIndex);

    float3 radiance = float3(0.0, 0.0, 0.0);

    for (uint i = 0; i < uniforms.Globals.SampleCount; i++)
    {
        RayDesc rayDesc = uniforms.Globals.Camera.GetRayDesc(float2(Random.Rnd(seed), Random.Rnd(seed)));

        Payload payload = {};
        for (uint j = 0; j < uniforms.Globals.MaxDepth; j++)
        {
            payload.Depth = j;
            payload.Stop = false;

            payload.Seed = seed;
            payload.Ray.Origin = rayDesc.Origin;
            payload.Ray.Direction = rayDesc.Direction;

            TraceRay(uniforms.Scene, RAY_FLAG_FORCE_OPAQUE, 0xFF, 0, 0, 0, rayDesc, payload);

            seed = payload.Seed;
            rayDesc.Origin = payload.Ray.Origin;
            rayDesc.Direction = payload.Ray.Direction;

            if (payload.Stop)
            {
                break;
            }
        }

        radiance += payload.Radiance;
    }

    radiance /= float(uniforms.Globals.SampleCount);

    // HDR scale
    float4 accumulated =
        uniforms.Globals.FrameIndex <= 1 ? float4(0.0, 0.0, 0.0, 0.0) : uniforms.Accumulation[launchID];
    float3 accumulatedRadiance = accumulated.xyz + radiance;

    uniforms.Accumulation[launchID] = float4(accumulatedRadiance, 1.0);

    float inv = 1.0 / float(uniforms.Globals.FrameIndex);
    radiance = accumulatedRadiance * inv;

    // LDR scale
    float3 ldr = Maths.ToneMap(radiance, 1.5);

    uniforms.Output[launchID] = float4(ldr, 1.0);
}
