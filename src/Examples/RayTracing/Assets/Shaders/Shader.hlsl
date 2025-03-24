﻿#include "Common.hlsl"

struct Vertex
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

struct AO
{
    float Radius;
    
    int Samples;
    
    float Power;
    
    bool DistanceBased;
};

struct Global
{
    Camera Camera;

    AO AO;
    
    int FrameNumber;
};

struct [raypayload] Payload
{
    bool Hit;

    float3 Color;
};

struct [raypayload] AOPayload
{
    float Value;
};

RaytracingAccelerationStructure Scene : register(t0, space0);
StructuredBuffer<Vertex> VertexBuffers[4] : register(t1, space0);
StructuredBuffer<uint> IndexBuffers[4] : register(t5, space0);
StructuredBuffer<Material> Materials : register(t9, space0);
ConstantBuffer<Global> Global : register(b0, space0);
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
    result.Normal = normalize(barycentrics.x * v0.Normal + barycentrics.y * v1.Normal + barycentrics.z * v2.Normal);
    result.TexCoord = barycentrics.x * v0.TexCoord + barycentrics.y * v1.TexCoord + barycentrics.z * v2.TexCoord;

    return result;
}

float TraceRayAO(float3 origin, float3 direction)
{
    RayDesc rayDesc;
    rayDesc.Origin = origin;
    rayDesc.Direction = direction;
    rayDesc.TMin = 0.001;
    rayDesc.TMax = Global.AO.Radius;

    AOPayload payload;
    TraceRay(Scene, RAY_FLAG_NONE, 0xFF, 1, 0, 1, rayDesc, payload);

    return payload.Value;
}

[shader("raygeneration")]
void RayGenMain()
{
    uint2 launchID = DispatchRaysIndex().xy;
    float2 launchSize = float2(DispatchRaysDimensions().xy);
    float aspectRatio = launchSize.x / launchSize.y;
    
    float2 xy = (launchID / launchSize * 2.0 - 1.0) * tan(Global.Camera.Fov * 0.5);

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
    rayDesc.Origin = Global.Camera.Position;
    rayDesc.Direction = normalize(Global.Camera.Forward + xy.x * Global.Camera.Right + xy.y * Global.Camera.Up);
    rayDesc.TMin = Global.Camera.NearPlane;
    rayDesc.TMax = Global.Camera.FarPlane;

    Payload payload;
    TraceRay(Scene, RAY_FLAG_FORCE_OPAQUE, 0xFF, 0, 0, 0, rayDesc, payload);

    if (payload.Hit)
    {
        if (Global.FrameNumber == 0)
        {
            Output[launchID] = float4(payload.Color, 1.0);
        }
        else
        {
            Output[launchID] = lerp(Output[launchID], float4(payload.Color, 1.0), 1.0 / (Global.FrameNumber + 1));
        }
    }
    else
    {
        Output[launchID] = float4(0.0, 0.0, 0.0, 1.0);
    }
}

[shader("miss")]
void MissMain(inout Payload payload)
{
    payload.Hit = false;
}

[shader("closesthit")]
void ClosestHitMain(inout Payload payload, in BuiltInTriangleIntersectionAttributes attrib)
{
    uint2 launchID = DispatchRaysIndex().xy;
    uint2 launchSize = DispatchRaysDimensions().xy;
    
    uint seed = tea(launchSize.x * launchID.y + launchID.x, Global.FrameNumber);

    uint geometryIndex = GeometryIndex();
    uint primitiveIndex = PrimitiveIndex();
    float3 barycentrics = float3(1.0 - attrib.barycentrics.x - attrib.barycentrics.y, attrib.barycentrics.x, attrib.barycentrics.y);
    
    Vertex vertex = GetVertex(geometryIndex, primitiveIndex, barycentrics);

    Material material = Materials[geometryIndex];
    
    payload.Hit = true;
    payload.Color = material.Albedo;

    if (material.IsLight)
    {
        payload.Color += material.Emission;
    }

    float3 worldPosition = WorldRayOrigin() + RayTCurrent() * WorldRayDirection();
    
    // AO

    {
        float ao = 0.0;

        float3 origin = OffsetRay(worldPosition, vertex.Normal);

        float3 x, y;
        ComputeDefaultBasis(vertex.Normal, x, y);

        for (int i = 0; i < Global.AO.Samples; i++)
        {
            float r1 = rnd(seed);
            float r2 = rnd(seed);
            float sq = sqrt(1.0 - r2);
            float phi = 2.0 * 3.14159265359 * r1;
            
            float3 direction = float3(cos(phi) * sq, sin(phi) * sq, sqrt(r2));
            direction = normalize(direction.x * x + direction.y * y + direction.z * vertex.Normal);

            ao += TraceRayAO(origin, direction);
        }

        ao = pow(1.0 - (ao / Global.AO.Samples), Global.AO.Power);

        payload.Color *= ao;
    }
}

[shader("miss")]
void MissAO(inout AOPayload payload)
{
    payload.Value = 0.0;
}

[shader("closesthit")]
void ClosestHitAO(inout AOPayload payload, in BuiltInTriangleIntersectionAttributes attrib)
{
    if (Global.AO.DistanceBased)
    {
        payload.Value = 1.0 - RayTCurrent() / Global.AO.Radius;
    }
    else
    {
        payload.Value = 1.0;
    }
}