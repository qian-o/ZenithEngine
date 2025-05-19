import "Common/Random";
import "Common/Structs";
import "Uniforms";

[shader("raygeneration")]
void Main()
{
    uint2 launchID = DispatchRaysIndex().xy;

    float3 color = float3(0.0, 0.0, 0.0);

    Payload payload;
    TraceRay(scene, RAY_FLAG_FORCE_OPAQUE, 0xFF, 0, 0, 0, globals.Camera.GetRayDesc(), payload);

    color += payload.Normal;

    output[launchID] = float4(color, 1.0);
}
