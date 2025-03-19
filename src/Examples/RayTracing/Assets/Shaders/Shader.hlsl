﻿struct Vertex
{
    float3 Position;
    
    float3 Normal;
    
    float2 TexCoord;
};

struct Material
{
    bool IsLight;

    float3 Albedo;

    float3 Emission;
};

struct Camera
{
    float3 Position;
    
    float3 Forward;
    
    float3 Right;
    
    float3 Up;
    
    float NearPlane;
    
    float FarPlane;
    
    float Fov;
};

struct AOParameters
{
    float Radius;
    
    int Samples;
    
    float Power;
    
    bool DistanceBased;
    
    int FrameNumber;
    
    int MaxSamples;
};

struct [raypayload] Payload
{
    bool Hit;
    
    float3 Normal;
    
    float2 TexCoord;

    float3 Color;
};

struct [raypayload] AOPayload
{
    float AO;
};

RaytracingAccelerationStructure Scene : register(t0, space0);
StructuredBuffer<Vertex> VertexBuffers[4] : register(t1, space0);
StructuredBuffer<uint> IndexBuffers[4] : register(t5, space0);
StructuredBuffer<Material> Materials : register(t9, space0);
ConstantBuffer<Camera> Camera : register(b0, space0);
ConstantBuffer<AOParameters> AOParameters : register(b1, space0);
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

float GetAO(float3 origin, float3 direction)
{
    RayDesc rayDesc;
    rayDesc.Origin = origin;
    rayDesc.Direction = direction;
    rayDesc.TMin = 0.001;
    rayDesc.TMax = AOParameters.Radius;

    AOPayload payload;
    TraceRay(Scene, RAY_FLAG_NONE, 0xFF, 1, 0, 1, rayDesc, payload);

    return payload.AO;
}

[shader("raygeneration")]
void RayGenMain()
{
    uint2 launchID = DispatchRaysIndex().xy;
    float2 launchSize = float2(DispatchRaysDimensions().xy);
    float aspectRatio = launchSize.x / launchSize.y;
    
    float2 xy = (launchID / launchSize * 2.0 - 1.0) * tan(Camera.Fov * 0.5);

    if (aspectRatio > 1.0)
    {
        xy.x *= aspectRatio;
    }
    else
    {
        xy.y /= aspectRatio;
    }

    xy.y = -xy.y;
    
    RayDesc rayDesc;
    rayDesc.Origin = Camera.Position;
    rayDesc.Direction = normalize(Camera.Forward + xy.x * Camera.Right + xy.y * Camera.Up);
    rayDesc.TMin = Camera.NearPlane;
    rayDesc.TMax = Camera.FarPlane;

    Payload payload;
    TraceRay(Scene, RAY_FLAG_FORCE_OPAQUE, 0xFF, 0, 0, 0, rayDesc, payload);

    Output[launchID] = payload.Hit ? float4(payload.TexCoord, 0.0, 1.0) : float4(0.0, 0.0, 0.0, 1.0);
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
    Material material = Materials[geometryIndex];

    payload.Hit = true;
    payload.Normal = normalize(vertex.Normal);
    payload.TexCoord = vertex.TexCoord;
    payload.Color = material.Albedo;
}

[shader("miss")]
void MissAO(inout AOPayload payload)
{
    payload.AO = 1.0;
}

[shader("closesthit")]
void ClosestHitAO(inout AOPayload payload, in BuiltInTriangleIntersectionAttributes attrib)
{
    if (AOParameters.DistanceBased)
    {
        payload.AO = 1.0;
    }
    else
    {
        payload.AO = 1.0 - RayTCurrent() / AOParameters.Radius;
    }
}