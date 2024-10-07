struct Camera
{
    float3 position;
    
    float3 forward;
    
    float3 right;
    
    float3 up;
    
    float nearPlane;
    
    float farPlane;
    
    float fov;
};

struct Other
{
    int antiAliasing;
    
    int lightCount;
};

struct Light
{
    float3 position;
    
    float3 intensity;
};

struct Vertex
{
    float3 position;
    
    float3 normal;
    
    float2 texCoord;
    
    float3 color;
    
    float4 tangent;
};

struct GeometryNode
{
    uint vertexBuffer;

    uint indexBuffer;
    
    bool alphaMask;
    
    float alphaCutoff;
    
    bool doubleSided;

    float4 baseColorFactor;
    
    uint baseColorTextureIndex;
    
    uint normalTextureIndex;
};

struct Payload
{
    float4 color;
};

struct ShadowRayPayload
{
    bool hit;
};

struct AORayPayload
{
    float aoValue;
};

RaytracingAccelerationStructure as : register(t0, space0);
ConstantBuffer<Camera> camera : register(b1, space0);
ConstantBuffer<Other> other : register(b2, space0);
StructuredBuffer<Light> lights : register(t3, space0);
StructuredBuffer<GeometryNode> geometryNodes : register(t4, space0);
RWTexture2D<float4> outputTexture : register(u5, space0);

StructuredBuffer<Vertex> vertexArray[] : register(t0, space1);
StructuredBuffer<uint> indexArray[] : register(t0, space2);
Texture2D textureArray[] : register(t0, space3);
SamplerState samplerArray[] : register(s0, space4);

float rangeMap(float value, float min, float max, float newMin, float newMax)
{
    return newMin + (value - min) * (newMax - newMin) / (max - min);
}

Vertex getVertex(StructuredBuffer<Vertex> vertexBuffer, StructuredBuffer<uint> indexBuffer, uint vertexIndex, float3 barycentrics)
{
    uint3 indices = uint3(indexBuffer[vertexIndex * 3], indexBuffer[vertexIndex * 3 + 1], indexBuffer[vertexIndex * 3 + 2]);
    
    Vertex v0 = vertexBuffer[indices.x];
    Vertex v1 = vertexBuffer[indices.y];
    Vertex v2 = vertexBuffer[indices.z];
    
    Vertex result;
    result.position = v0.position * barycentrics.x + v1.position * barycentrics.y + v2.position * barycentrics.z;
    result.normal = v0.normal * barycentrics.x + v1.normal * barycentrics.y + v2.normal * barycentrics.z;
    result.texCoord = v0.texCoord * barycentrics.x + v1.texCoord * barycentrics.y + v2.texCoord * barycentrics.z;
    result.color = v0.color * barycentrics.x + v1.color * barycentrics.y + v2.color * barycentrics.z;
    result.tangent = v0.tangent * barycentrics.x + v1.tangent * barycentrics.y + v2.tangent * barycentrics.z;
    
    return result;
}

float3 hitWorldPosition()
{
    return WorldRayOrigin() + RayTCurrent() * WorldRayDirection();
}

[shader("raygeneration")]
void rayGen()
{
    uint3 LaunchID = DispatchRaysIndex();
    uint3 LaunchSize = DispatchRaysDimensions();
    
    float x = LaunchID.x + 0.5;
    float y = LaunchID.y + 0.5;
    
    float scale = tan(camera.fov);
    float aspectRatio = LaunchSize.x / float(LaunchSize.y);
    
    x = rangeMap(x, 0, LaunchSize.x - 1, -1.0, 1.0);
    y = rangeMap(y, 0, LaunchSize.y - 1, 1.0, -1.0);

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
    rayDesc.Origin = camera.position;
    rayDesc.Direction = normalize(camera.forward + x * camera.right + y * camera.up);
    rayDesc.TMin = camera.nearPlane;
    rayDesc.TMax = camera.farPlane;
    
    Payload payload;
    TraceRay(as, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xff, 0, 0, 0, rayDesc, payload);
    
    outputTexture[LaunchID.xy] = payload.color;

}

[shader("miss")]
void miss(inout Payload payload)
{
    payload.color = float4(0.0, 0.0, 0.2, 1.0);
}

[shader("miss")]
void missShadow(inout ShadowRayPayload payload)
{
    payload.hit = false;
}

[shader("closesthit")]
void closestHit(inout Payload payload, in BuiltInTriangleIntersectionAttributes attribs)
{
    float3 barycentricCoords = float3(1.0f - attribs.barycentrics.x - attribs.barycentrics.y, attribs.barycentrics.x, attribs.barycentrics.y);
    
    GeometryNode node = geometryNodes[GeometryIndex()];
    
    Vertex vertex = getVertex(vertexArray[node.vertexBuffer], indexArray[node.indexBuffer], PrimitiveIndex(), barycentricCoords);
    vertex.position = hitWorldPosition();
    
    float4 baseColor = textureArray[node.baseColorTextureIndex].SampleLevel(samplerArray[0], vertex.texCoord, 0) * float4(vertex.color, 1.0);
    
    float3 finalColor = float3(0.0, 0.0, 0.0);
    
    for (int i = 0; i < other.lightCount; ++i)
    {
        Light light = lights[i];
        
        float3 lightDir = normalize(light.position - vertex.position);
        float3 normal = normalize(vertex.normal);
        
        float diff = max(dot(normal, lightDir), 0.0);
        float3 diffuse = diff * light.intensity;
        
        RayDesc shadowRay;
        shadowRay.Origin = vertex.position;
        shadowRay.Direction = lightDir;
        shadowRay.TMin = 0.001;
        shadowRay.TMax = length(light.position - vertex.position);
        
        ShadowRayPayload shadowPayload;
        shadowPayload.hit = true;
        TraceRay(as, RAY_FLAG_ACCEPT_FIRST_HIT_AND_END_SEARCH, 0xff, 1, 0, 1, shadowRay, shadowPayload);
        
        if (shadowPayload.hit)
        {
            finalColor += baseColor.rgb * 0.05;
        }
        else
        {
            finalColor += diffuse * baseColor.rgb;
        }
    }
    
    payload.color = float4(finalColor, baseColor.a);
}

[shader("closesthit")]
void shadowChs(inout ShadowRayPayload payload, in BuiltInTriangleIntersectionAttributes attribs)
{
    payload.hit = true;
}

[shader("anyhit")]
void anyHit(inout Payload payload, in BuiltInTriangleIntersectionAttributes attribs)
{
    float3 barycentricCoords = float3(1.0f - attribs.barycentrics.x - attribs.barycentrics.y, attribs.barycentrics.x, attribs.barycentrics.y);
    
    GeometryNode node = geometryNodes[GeometryIndex()];
    
    Vertex vertex = getVertex(vertexArray[node.vertexBuffer], indexArray[node.indexBuffer], PrimitiveIndex(), barycentricCoords);
    
    float4 color = textureArray[node.baseColorTextureIndex].SampleLevel(samplerArray[0], vertex.texCoord, 0) * float4(vertex.color, 1.0);
    
    if (node.alphaMask && color.a < node.alphaCutoff)
    {
        IgnoreHit();
    }
}
