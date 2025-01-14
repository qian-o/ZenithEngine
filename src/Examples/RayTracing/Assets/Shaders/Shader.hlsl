struct Vertex
{
    float3 Position;
    
    float3 Normal;
    
    float2 TexCoord;
};

struct [raypayload] Payload
{
    float3 Normal;
    
    float2 TexCoord;
};

RaytracingAccelerationStructure Scene : register(t0, space0);
StructuredBuffer<Vertex> VertexBuffer : register(t1, space0);
StructuredBuffer<uint> IndexBuffer : register(t2, space0);
RWTexture2D<float4> Output : register(u0, space0);

[shader("raygeneration")]
void RayGenMain()
{
    uint3 LaunchID = DispatchRaysIndex();
    uint3 LaunchSize = DispatchRaysDimensions();
    
    float x = float(LaunchID.x) / float(LaunchSize.x);
    float y = float(LaunchID.y) / float(LaunchSize.y);
    
    x = x * 2.0 - 1.0;
    y = 1.0 - y * 2.0;

    RayDesc rayDesc;
    rayDesc.Origin = float3(0.0, 0.0, 0.0);
    rayDesc.Direction = normalize(float3(x, y, 1.0f));
    rayDesc.TMin = 0.001;
    rayDesc.TMax = 1000.0;

    Payload payload;
    TraceRay(Scene, RAY_FLAG_ACCEPT_FIRST_HIT_AND_END_SEARCH, 0xFF, 0, 1, 0, rayDesc, payload);

    Output[LaunchID.xy] = float4(payload.Normal, 1.0);
}

[shader("miss")]
void MissMain(inout Payload payload)
{
    payload.Normal = float3(0.0, 0.0, 0.0);
    payload.TexCoord = float2(0.0, 0.0);
}

[shader("closesthit")]
void ClosestHitMain(inout Payload payload, in BuiltInTriangleIntersectionAttributes attrib)
{
    uint primitiveIndex = PrimitiveIndex();
    
    float3 barycentrics = float3(1.0 - attrib.barycentrics.x - attrib.barycentrics.y, attrib.barycentrics.x, attrib.barycentrics.y);
    
    uint3 indices = uint3(IndexBuffer[primitiveIndex * 3], IndexBuffer[primitiveIndex * 3 + 1], IndexBuffer[primitiveIndex * 3 + 2]);
    
    Vertex v0 = VertexBuffer[indices.x];
    Vertex v1 = VertexBuffer[indices.y];
    Vertex v2 = VertexBuffer[indices.z];
    
    payload.Normal = normalize(barycentrics.x * v0.Normal + barycentrics.y * v1.Normal + barycentrics.z * v2.Normal);
    payload.TexCoord = barycentrics.x * v0.TexCoord + barycentrics.y * v1.TexCoord + barycentrics.z * v2.TexCoord;
}