import "Common/Random";
import "Common/Structs";
import "Uniforms";

[shader("miss")]
void Main(inout Payload payload)
{
    payload.Normal = float3(0.0, 0.0, 1.0);
}