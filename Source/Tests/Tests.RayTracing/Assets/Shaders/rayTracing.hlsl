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

RaytracingAccelerationStructure as : register(t0, space0);
StructuredBuffer<GeometryNode> geometryNodes : register(t1, space0);
RWTexture2D<float4> outputTexture : register(u2, space0);

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

[shader("raygeneration")]
void rayGen()
{
    const float fov = 45.0;
    const float3 position = float3(0, 2.0, -2.0);
    const float3 forward = normalize(float3(0, 0, 1));
    const float3 right = normalize(cross(float3(0, 1, 0), forward));
    const float3 up = cross(forward, right);
    
    uint3 LaunchID = DispatchRaysIndex();
    uint3 LaunchSize = DispatchRaysDimensions();
    
    float x = LaunchID.x + 0.5;
    float y = LaunchID.y + 0.5;
    
    float scale = tan(radians(fov));
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
    
    float3 direction = normalize(forward + x * right + y * up);
    
    RayDesc rayDesc;
    rayDesc.Origin = position;
    rayDesc.Direction = direction;
    rayDesc.TMin = 0.001;
    rayDesc.TMax = 10000.0;
    
    Payload payload;
    TraceRay(as, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xff, 0, 0, 0, rayDesc, payload);
    
    outputTexture[LaunchID.xy] = payload.color;

}

[shader("miss")]
void miss(inout Payload payload)
{
    payload.color = float4(0.0, 0.0, 0.2, 1.0);
}

[shader("closesthit")]
void closestHit(inout Payload payload, in BuiltInTriangleIntersectionAttributes attribs)
{
    float3 barycentricCoords = float3(1.0f - attribs.barycentrics.x - attribs.barycentrics.y, attribs.barycentrics.x, attribs.barycentrics.y);
    
    GeometryNode node = geometryNodes[GeometryIndex()];
    
    Vertex vertex = getVertex(vertexArray[node.vertexBuffer], indexArray[node.indexBuffer], PrimitiveIndex(), barycentricCoords);

    float4 color = textureArray[node.baseColorTextureIndex].SampleLevel(samplerArray[0], vertex.texCoord, 0) * float4(vertex.color, 1.0);
    
    payload.color = color;
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
