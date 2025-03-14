﻿struct Camera
{
    float3 Position;
    
    float3 Forward;
    
    float3 Right;
    
    float3 Up;
    
    float NearPlane;
    
    float FarPlane;
    
    float Fov;
};

struct Vertex
{
    float3 Position;
    
    float3 Normal;
    
    float2 TexCoord;
};

struct [raypayload] Payload
{
    bool Hit;
    
    float3 Normal;
    
    float2 TexCoord;
};

RaytracingAccelerationStructure Scene : register(t0, space0);
StructuredBuffer<Vertex> VertexBuffers[4] : register(t1, space0);
StructuredBuffer<uint> IndexBuffers[4] : register(t5, space0);
ConstantBuffer<Camera> Camera : register(b0, space0);
RWTexture2D<float4> Output : register(u0, space0);

Vertex GetVertex(uint geometryIndex, uint primitiveIndex, float3 barycentrics)
{
    StructuredBuffer<Vertex> vertexBuffer = VertexBuffers[geometryIndex];
    StructuredBuffer<uint> indexBuffer = IndexBuffers[geometryIndex];

    uint3 indices = uint3(indexBuffer[primitiveIndex * 3], indexBuffer[primitiveIndex * 3 + 1], indexBuffer[primitiveIndex * 3 + 2]);

    Vertex v0 = vertexBuffer[indices.x];
    Vertex v1 = vertexBuffer[indices.y];
    Vertex v2 = vertexBuffer[indices.z];

    Vertex result;
    result.Position = barycentrics.x * v0.Position + barycentrics.y * v1.Position + barycentrics.z * v2.Position;
    result.Normal = barycentrics.x * v0.Normal + barycentrics.y * v1.Normal + barycentrics.z * v2.Normal;
    result.TexCoord = barycentrics.x * v0.TexCoord + barycentrics.y * v1.TexCoord + barycentrics.z * v2.TexCoord;

    return result;
}

[shader("raygeneration")]
void RayGenMain()
{
    uint3 LaunchID = DispatchRaysIndex();
    uint3 LaunchSize = DispatchRaysDimensions();
    
    float x = float(LaunchID.x) / float(LaunchSize.x);
    float y = float(LaunchID.y) / float(LaunchSize.y);
    
    x = x * 2.0 - 1.0;
    y = 1.0 - y * 2.0;
    
    float scale = tan(Camera.Fov);
    float aspectRatio = LaunchSize.x / float(LaunchSize.y);

    if (aspectRatio > 1.0)
    {
        x *= aspectRatio * scale;
        y *= scale;
    }
    else
    {
        x *= scale;
        y *= scale / aspectRatio;
    }
    
    RayDesc rayDesc;
    rayDesc.Origin = Camera.Position;
    rayDesc.Direction = normalize(Camera.Forward + x * Camera.Right + y * Camera.Up);
    rayDesc.TMin = Camera.NearPlane;
    rayDesc.TMax = Camera.FarPlane;

    Payload payload;
    TraceRay(Scene, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF, 0, 0, 0, rayDesc, payload);

    Output[LaunchID.xy] = payload.Hit ? float4(payload.TexCoord, 1.0, 1.0) : float4(0.0, 0.0, 0.0, 1.0);
}

[shader("miss")]
void MissMain(inout Payload payload)
{
    payload.Hit = false;
    payload.Normal = float3(0.0, 0.0, 0.0);
    payload.TexCoord = float2(0.0, 0.0);
}

[shader("closesthit")]
void ClosestHitMain(inout Payload payload, in BuiltInTriangleIntersectionAttributes attrib)
{
    uint geometryIndex = GeometryIndex();
    uint primitiveIndex = PrimitiveIndex();
    float3 barycentrics = float3(1.0 - attrib.barycentrics.x - attrib.barycentrics.y, attrib.barycentrics.x, attrib.barycentrics.y);
    
    Vertex vertex = GetVertex(geometryIndex, primitiveIndex, barycentrics);

    payload.Hit = true;
    payload.Normal = normalize(vertex.Normal);
    payload.TexCoord = vertex.TexCoord;
}