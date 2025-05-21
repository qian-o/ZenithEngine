import "Common/Structs";
import "Common/Uniforms";

[shader("miss")]
void Main(inout Payload payload)
{
    payload.Normal = float3(0.0, 0.0, 1.0);
}

[shader("miss")]
void ShadowMain(inout ShadowPayload payload)
{
    payload.InShadow = false;
}