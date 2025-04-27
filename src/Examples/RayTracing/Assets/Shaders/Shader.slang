import "Common";

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

struct Payload
{
    bool Hit;

    float3 Color;
};

struct AOPayload
{
    float Value;
};

RaytracingAccelerationStructure scene : register(t0, space0);
StructuredBuffer<Vertex> vertexBuffers[4] : register(t1, space0);
StructuredBuffer<uint> indexBuffers[4] : register(t5, space0);
StructuredBuffer<Material> materials : register(t9, space0);
ConstantBuffer<Global> global : register(b0, space0);

[[vk::image_format("rgba8")]]
RWTexture2D<float4> Output : register(u0, space0);

Vertex GetVertex(uint geometryIndex, uint primitiveIndex, float3 barycentrics)
{
    StructuredBuffer<Vertex> vertexBuffer = vertexBuffers[geometryIndex];
    StructuredBuffer<uint> indexBuffer = indexBuffers[geometryIndex];

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
    rayDesc.TMax = global.AO.Radius;

    AOPayload payload;
    TraceRay(scene, RAY_FLAG_NONE, 0xFF, 1, 0, 1, rayDesc, payload);

    return payload.Value;
}

[shader("raygeneration")]
void RayGenMain()
{
    const uint spp = 8;

    uint2 launchID = DispatchRaysIndex().xy;
    uint2 launchSize = DispatchRaysDimensions().xy;

    uint seed = tea(launchSize.x * launchID.y + launchID.x, global.FrameNumber);

    float aspectRatio = (float)launchSize.x / launchSize.y;

    float3 color = float3(0.0, 0.0, 0.0);

    for (uint i = 0; i < spp; i++)
    {
        float2 xy = ((launchID + float2(rnd(seed), rnd(seed))) / launchSize * 2.0 - 1.0) * tan(global.Camera.Fov * 0.5);

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
        rayDesc.Origin = global.Camera.Position;
        rayDesc.Direction = normalize(global.Camera.Forward + xy.x * global.Camera.Right + xy.y * global.Camera.Up);
        rayDesc.TMin = global.Camera.NearPlane;
        rayDesc.TMax = global.Camera.FarPlane;

        Payload payload;
        TraceRay(scene, RAY_FLAG_FORCE_OPAQUE, 0xFF, 0, 0, 0, rayDesc, payload);
        
        color += payload.Color;
    }
    
    color /= spp;

    if (global.FrameNumber == 0)
    {
        Output[launchID] = float4(color, 1.0);
    }
    else
    {
        Output[launchID] = lerp(Output[launchID], float4(color, 1.0), 1.0 / (global.FrameNumber + 1));
    }
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
    uint2 launchID = DispatchRaysIndex().xy;
    uint2 launchSize = DispatchRaysDimensions().xy;
    
    uint seed = tea(launchSize.x * launchID.y + launchID.x, global.FrameNumber);

    uint geometryIndex = GeometryIndex();
    uint primitiveIndex = PrimitiveIndex();
    float3 barycentrics = float3(1.0 - attrib.barycentrics.x - attrib.barycentrics.y, attrib.barycentrics.x, attrib.barycentrics.y);
    
    Vertex vertex = GetVertex(geometryIndex, primitiveIndex, barycentrics);

    Material material = materials[geometryIndex];
    
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

        for (int i = 0; i < global.AO.Samples; i++)
        {
            float r1 = rnd(seed);
            float r2 = rnd(seed);
            float sq = sqrt(1.0 - r2);
            float phi = 2.0 * 3.14159265359 * r1;
            
            float3 direction = float3(cos(phi) * sq, sin(phi) * sq, sqrt(r2));
            direction = normalize(direction.x * x + direction.y * y + direction.z * vertex.Normal);

            ao += TraceRayAO(origin, direction);
        }

        ao = pow(1.0 - (ao / global.AO.Samples), global.AO.Power);

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
    if (global.AO.DistanceBased)
    {
        payload.Value = 1.0 - RayTCurrent() / global.AO.Radius;
    }
    else
    {
        payload.Value = 1.0;
    }
}