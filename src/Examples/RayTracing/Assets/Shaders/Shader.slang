import "Common/Random";
import "Common/Structs";

struct Payload
{
    bool Hit;

    float3 Color;
};

uniform RaytracingAccelerationStructure scene;
uniform StructuredBuffer<Vertex> vertexBuffers[4];
uniform StructuredBuffer<uint> indexBuffers[4];
uniform StructuredBuffer<Material> materials;
uniform Globals globals;
uniform RWTexture2D output;

[shader("raygeneration")]
void RayGenMain()
{
    uint2 launchID = DispatchRaysIndex().xy;

    float3 color = float3(0.0, 0.0, 0.0);

    Payload payload;
    TraceRay(scene, RAY_FLAG_FORCE_OPAQUE, 0xFF, 0, 0, 0, globals.Camera.GetRayDesc(), payload);

    color += payload.Color;

    output[launchID] = float4(color, 1.0);
}

[shader("miss")]
void MissMain(inout Payload payload)
{
    payload.Hit = false;
    payload.Color = float3(0.0, 0.0, 0.0);
}

[shader("closesthit")]
void ClosestHitMain(inout Payload payload, in BuiltInTriangleIntersectionAttributes attrib)
{
    Vertex vertex = Vertex.GetVertex(vertexBuffers, indexBuffers, attrib);

    payload.Hit = true;
    payload.Color = normalize(vertex.Normal) * 0.5 + 0.5;
}
