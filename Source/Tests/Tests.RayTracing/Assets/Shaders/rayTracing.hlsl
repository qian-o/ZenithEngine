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
    int pathTracerSampleIndex;
    
    float pathTracerAccumulationFactor;
    
    float2 pixelOffset;
    
    int lightCount;
    
    int numRays;
    
    int frameCount;
    
    int numBounces;
};

struct Light
{
    float3 position;
    
    float4 ambientColor;
    
    float4 diffuseColor;
    
    float4 specularColor;
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

struct Ray
{
    float3 origin;
    float3 direction;
    float min;
    float max;
};

struct RayPayload
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

struct GIRayPayload
{
    float3 color;
    uint depth;
    uint seed;
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

static float4 backgroundColor = float4(0.09, 0.08, 0.14, 1.0);

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

uint initRand(uint val0, uint val1, uint backoff = 16)
{
    uint v0 = val0, v1 = val1, s0 = 0;

	[unroll]
    for (uint n = 0; n < backoff; n++)
    {
        s0 += 0x9e3779b9;
        v0 += ((v1 << 4) + 0xa341316c) ^ (v1 + s0) ^ ((v1 >> 5) + 0xc8013ea4);
        v1 += ((v0 << 4) + 0xad90777d) ^ (v0 + s0) ^ ((v0 >> 5) + 0x7e95761e);
    }
    return v0;
}

// Takes our seed, updates it, and returns a pseudorandom float in [0..1]
float nextRand(inout uint s)
{
    s = (1664525u * s + 1013904223u);
    return float(s & 0x00FFFFFF) / float(0x01000000);
}

// Utility function to get a vector perpendicular to an input vector 
//    (from "Efficient Construction of Perpendicular Vectors Without Branching")
float3 getPerpendicularVector(float3 u)
{
    float3 a = abs(u);
    uint xm = ((a.x - a.y) < 0 && (a.x - a.z) < 0) ? 1 : 0;
    uint ym = (a.y - a.z) < 0 ? (1 ^ xm) : 0;
    uint zm = 1 ^ (xm | ym);
    return cross(u, float3(xm, ym, zm));
}

// Get a cosine-weighted random vector centered around a specified normal direction.
float3 getCosHemisphereSample(inout uint randSeed, float3 hitNorm)
{
	// Get 2 random numbers to select our sample with
    float2 randVal = float2(nextRand(randSeed), nextRand(randSeed));

	// Cosine weighted hemisphere sample from RNG
    float3 bitangent = getPerpendicularVector(hitNorm);
    float3 tangent = cross(bitangent, hitNorm);
    float r = sqrt(randVal.x);
    float phi = 2.0f * 3.14159265f * randVal.y;

	// Get our cosine-weighted hemisphere lobe sample direction
    return tangent * (r * cos(phi).x) + bitangent * (r * sin(phi)) + hitNorm.xyz * sqrt(1 - randVal.x);
}

float3 traceGIRay(Ray ray, uint depth, uint seed)
{
    RayDesc rayDesc;
    rayDesc.Origin = ray.origin;
    rayDesc.Direction = ray.direction;
    rayDesc.TMin = ray.min;
    rayDesc.TMax = ray.max;
    
    GIRayPayload payload;
    payload.color = float3(0, 0, 0);
    payload.depth = depth + 1;
    payload.seed = seed;
    
    TraceRay(as, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xff, 3, 0, 3, rayDesc, payload);
    
    return payload.color;
}

float traceAORay(Ray ray)
{
    RayDesc rayDesc;
    rayDesc.Origin = ray.origin;
    rayDesc.Direction = ray.direction;
    rayDesc.TMin = ray.min;
    rayDesc.TMax = ray.max;
    
    AORayPayload payload;
    TraceRay(as, RAY_FLAG_NONE, 0xff, 2, 0, 2, rayDesc, payload);
    
    return payload.aoValue;
}

bool traceShadowRay(Ray ray)
{
    RayDesc rayDesc;
    rayDesc.Origin = ray.origin;
    rayDesc.Direction = ray.direction;
    rayDesc.TMin = ray.min;
    rayDesc.TMax = ray.max;
    
    ShadowRayPayload payload;
    TraceRay(as, RAY_FLAG_ACCEPT_FIRST_HIT_AND_END_SEARCH, 0xff, 1, 0, 1, rayDesc, payload);
    
    return payload.hit;
}

float4 traceRadianceRay(Ray ray)
{
    RayDesc rayDesc;
    rayDesc.Origin = ray.origin;
    rayDesc.Direction = ray.direction;
    rayDesc.TMin = ray.min;
    rayDesc.TMax = ray.max;
    
    RayPayload payload;
    TraceRay(as, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xff, 0, 0, 0, rayDesc, payload);
    
    return payload.color;
}

float calculateDiffuseCoefficient(in float3 hitPosition, in float3 incidentLightRay, in float3 normal)
{
    float fNDotL = saturate(dot(-incidentLightRay, normal));
    return fNDotL;
}

float4 calculateSpecularCoefficient(in float3 hitPosition, in float3 incidentLightRay, in float3 normal, in float specularPower)
{
    float3 reflectedLightRay = normalize(reflect(incidentLightRay, normal));
    return pow(saturate(dot(reflectedLightRay, normalize(-WorldRayDirection()))), specularPower);
}

float4 calculatePhongLighting(in float4 albedo, in float3 normal, in float diffuseCoef, in float specularCoef, in float specularPower)
{
    float3 hitPosition = hitWorldPosition();
    
    float4 finalColor = float4(0, 0, 0, 0);
    
    for (int i = 0; i < other.lightCount; ++i)
    {
        Light light = lights[i];
        
        Ray shadowRay;
        shadowRay.origin = hitPosition;
        shadowRay.direction = normalize(light.position - hitPosition);
        shadowRay.min = 0.001;
        shadowRay.max = length(light.position - hitPosition);
        
        bool isInShadow = traceShadowRay(shadowRay);
        
        float shadowFactor = isInShadow ? 0.0 : 1.0;
        
        float3 incidentLightRay = normalize(hitPosition - light.position);
        
        float Kd = calculateDiffuseCoefficient(hitPosition, incidentLightRay, normal);
        float4 diffuseColor = shadowFactor * diffuseCoef * Kd * light.diffuseColor * albedo;
        
        float4 specularColor = float4(0, 0, 0, 0);
        if (!isInShadow)
        {
            float4 lightSpecularColor = float4(1, 1, 1, 1);
            float4 Ks = calculateSpecularCoefficient(hitPosition, incidentLightRay, normal, specularPower);
            specularColor = specularCoef * Ks * light.specularColor;
        }
        
        float4 ambientColor = light.ambientColor;
        float4 ambientColorMin = light.ambientColor - 0.1;
        float4 ambientColorMax = light.ambientColor;
        float a = 1 - saturate(dot(normal, float3(0, -1, 0)));
        ambientColor = albedo * lerp(ambientColorMin, ambientColorMax, a);
        
        finalColor += diffuseColor + specularColor + ambientColor;

    }
    
    return finalColor / other.lightCount;
}

[shader("raygeneration")]
void rayGen()
{
    uint3 LaunchID = DispatchRaysIndex();
    uint3 LaunchSize = DispatchRaysDimensions();
    
    float x = LaunchID.x + other.pixelOffset.x;
    float y = LaunchID.y + other.pixelOffset.y;
    
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
    
    Ray ray;
    ray.origin = camera.position;
    ray.direction = normalize(camera.forward + x * camera.right + y * camera.up);
    ray.min = camera.nearPlane;
    ray.max = camera.farPlane;
    
    float4 color = traceRadianceRay(ray);
    
    if (other.pathTracerSampleIndex == 0)
    {
        outputTexture[LaunchID.xy] = 0;
    }
    
    outputTexture[LaunchID.xy] = lerp(outputTexture[LaunchID.xy], float4(color.rgb, 1), other.pathTracerAccumulationFactor);
}

[shader("miss")]
void miss(inout RayPayload payload)
{
    payload.color = backgroundColor;
}

[shader("miss")]
void missShadow(inout ShadowRayPayload payload)
{
    payload.hit = false;
}

[shader("miss")]
void missAO(inout AORayPayload payload)
{
	// Our ambient occlusion value is 1 if we hit nothing.
    payload.aoValue = 1.0f;
}

[shader("miss")]
void missGI(inout GIRayPayload payload)
{
    payload.color = backgroundColor.rgb;
}

[shader("closesthit")]
void closestHit(inout RayPayload payload, in BuiltInTriangleIntersectionAttributes attribs)
{
	// Where is this thread's ray on screen?
    uint2 launchIndex = DispatchRaysIndex().xy;
    uint2 launchDim = DispatchRaysDimensions().xy;

	// Initialize a random seed, per-pixel, based on a screen position and temporally varying count
    uint randSeed = initRand(launchIndex.x + launchIndex.y * launchDim.x, other.frameCount, 16);
    
    float3 barycentricCoords = float3(1.0f - attribs.barycentrics.x - attribs.barycentrics.y, attribs.barycentrics.x, attribs.barycentrics.y);
    
    GeometryNode node = geometryNodes[GeometryIndex()];
    
    Vertex vertex = getVertex(vertexArray[node.vertexBuffer], indexArray[node.indexBuffer], PrimitiveIndex(), barycentricCoords);
    vertex.position = hitWorldPosition();
    
    float4 albedo = textureArray[node.baseColorTextureIndex].SampleLevel(samplerArray[0], vertex.texCoord, 0) * float4(vertex.color, 1.0);
    
    float4 color = calculatePhongLighting(albedo, vertex.normal, 0.9, 0.7, 50);
    
    float ambientOcclusion = 0.0f;
    for (int i = 0; i < other.numRays; i++)
    {
		// Sample cosine-weighted hemisphere around surface normal to pick a random ray direction
        float3 aoDir = getCosHemisphereSample(randSeed, vertex.normal);
        
        Ray ray;
        ray.origin = vertex.position;
        ray.direction = aoDir;
        ray.min = 0.01;
        ray.max = 0.4;
        
        ambientOcclusion += traceAORay(ray);
    }
    
    float aoColor = ambientOcclusion / float(other.numRays);
    color *= aoColor;
    
    if (other.numBounces > 0)
    {
        float3 giDir = getCosHemisphereSample(randSeed, vertex.normal);
        
        Ray ray;
        ray.origin = vertex.position;
        ray.direction = giDir;
        ray.min = camera.nearPlane;
        ray.max = camera.farPlane;
        
        float3 giColor = traceGIRay(ray, 0, randSeed);
        
        color.rgb += albedo.rgb * giColor;
    }

    payload.color = color;
}

[shader("closesthit")]
void shadowChs(inout ShadowRayPayload payload, in BuiltInTriangleIntersectionAttributes attribs)
{
    payload.hit = true;
}

[shader("closesthit")]
void aoChs(inout AORayPayload payload, in BuiltInTriangleIntersectionAttributes attribs)
{
    payload.aoValue = 0.0f;
}

[shader("closesthit")]
void giChs(inout GIRayPayload payload, in BuiltInTriangleIntersectionAttributes attribs)
{
    float3 barycentricCoords = float3(1.0f - attribs.barycentrics.x - attribs.barycentrics.y, attribs.barycentrics.x, attribs.barycentrics.y);
    
    GeometryNode node = geometryNodes[GeometryIndex()];
    
    Vertex vertex = getVertex(vertexArray[node.vertexBuffer], indexArray[node.indexBuffer], PrimitiveIndex(), barycentricCoords);
    vertex.position = hitWorldPosition();
    
    float4 albedo = textureArray[node.baseColorTextureIndex].SampleLevel(samplerArray[0], vertex.texCoord, 0) * float4(vertex.color, 1.0);
    
    float4 color = calculatePhongLighting(albedo, vertex.normal, 0.9, 0.7, 50);
    
    float ambientOcclusion = 0.0f;
    for (int i = 0; i < other.numRays; i++)
    {
		// Sample cosine-weighted hemisphere around surface normal to pick a random ray direction
        float3 aoDir = getCosHemisphereSample(payload.seed, vertex.normal);
        
        Ray ray;
        ray.origin = vertex.position;
        ray.direction = aoDir;
        ray.min = 0.01;
        ray.max = 0.4;
        
        ambientOcclusion += traceAORay(ray);
    }
    
    float aoColor = ambientOcclusion / float(other.numRays);
    color *= aoColor;

    payload.color = color.rgb;
    
    if (payload.depth < other.numBounces)
    {
        float3 giDir = getCosHemisphereSample(payload.seed, vertex.normal);
        
        Ray ray;
        ray.origin = vertex.position;
        ray.direction = giDir;
        ray.min = camera.nearPlane;
        ray.max = camera.farPlane;
        
        float3 giColor = traceGIRay(ray, payload.depth, payload.seed);
        
        payload.color += albedo.rgb * giColor;
    }
}

[shader("anyhit")]
void anyHit(inout RayPayload payload, in BuiltInTriangleIntersectionAttributes attribs)
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
