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

Vertex GetVertex(BuiltInTriangleIntersectionAttributes attrib)
{
    uint geometryIndex = GeometryIndex();
    uint primitiveIndex = PrimitiveIndex();
    float3 barycentrics =
        float3(1.0 - attrib.barycentrics.x - attrib.barycentrics.y, attrib.barycentrics.x, attrib.barycentrics.y);

    StructuredBuffer<Vertex> vertexBuffer = vertexBuffers[geometryIndex];
    StructuredBuffer<uint> indexBuffer = indexBuffers[geometryIndex];

    uint3 indices = uint3(indexBuffer[primitiveIndex * 3], indexBuffer[primitiveIndex * 3 + 1],
                          indexBuffer[primitiveIndex * 3 + 2]);

    Vertex v0 = vertexBuffer[indices.x];
    Vertex v1 = vertexBuffer[indices.y];
    Vertex v2 = vertexBuffer[indices.z];

    Vertex result;
    result.Position = barycentrics.x * v0.Position + barycentrics.y * v1.Position + barycentrics.z * v2.Position;
    result.Normal = normalize(barycentrics.x * v0.Normal + barycentrics.y * v1.Normal + barycentrics.z * v2.Normal);
    result.TexCoord = barycentrics.x * v0.TexCoord + barycentrics.y * v1.TexCoord + barycentrics.z * v2.TexCoord;

    return result;
}

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
    Vertex vertex = GetVertex(attrib);

    payload.Hit = true;
    payload.Color = normalize(vertex.Normal) * 0.5 + 0.5;
}
