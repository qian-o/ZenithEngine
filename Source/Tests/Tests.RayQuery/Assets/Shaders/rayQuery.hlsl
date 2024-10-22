struct GeometryNode
{
    float4x4 transform;

    bool alphaMask;

    float alphaCutoff;

    bool doubleSided;

    float4 baseColorFactor;

    uint baseColorTextureIndex;

    uint normalTextureIndex;

    uint roughnessTextureIndex;
};

struct Camera
{
    float3 position;

    float3 forward;

    float3 right;

    float3 up;

    float nearPlane;

    float farPlane;

    float fov;

    float4x4 view;

    float4x4 projection;
};

struct Light
{
    float3 position;

    float4 ambientColor;

    float4 diffuseColor;
};

struct VSInput
{
    [[vk::location(0)]] float3 Position : POSITION0;
    [[vk::location(1)]] float3 Normal : NORMAL0;
    [[vk::location(2)]] float2 TexCoord : TEXCOORD0;
    [[vk::location(3)]] float3 Color : COLOR0;
    [[vk::location(4)]] float4 Tangent : TEXCOORD1;
    [[vk::location(5)]] uint NodeIndex : TEXCOORD2;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    [[vk::location(0)]] float3 WorldPosition : POSITION0;
    [[vk::location(1)]] float3 Normal : NORMAL0;
    [[vk::location(2)]] float2 TexCoord : TEXCOORD0;
    [[vk::location(3)]] float3 Color : COLOR0;
    [[vk::location(4)]] float4 Tangent : TEXCOORD1;
    [[vk::location(5)]] nointerpolation uint NodeIndex : TEXCOORD2;
};

RaytracingAccelerationStructure as : register(t0, space0);
StructuredBuffer<GeometryNode> geometryNodes : register(t1, space0);
ConstantBuffer<Camera> camera : register(b2, space0);
StructuredBuffer<Light> lights : register(t3, space0);
Texture2D textureArray[] : register(t0, space1);
SamplerState samplerArray[] : register(s0, space2);

VSOutput mainVS(VSInput input)
{
    GeometryNode node = geometryNodes[input.NodeIndex];

    float4 worldPosition = mul(node.transform, float4(input.Position, 1.0));

    VSOutput output;
    output.Position = mul(camera.projection, mul(camera.view, worldPosition));
    output.WorldPosition = worldPosition.xyz;
    output.Normal = normalize(mul(node.transform, float4(input.Normal, 0.0)).xyz);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    output.Tangent = input.Tangent;
    output.NodeIndex = input.NodeIndex;

    return output;
}

float4 mainPS(VSOutput input) : SV_TARGET
{
    GeometryNode node = geometryNodes[input.NodeIndex];
    
    float4 color = textureArray[node.baseColorTextureIndex].Sample(samplerArray[0], input.TexCoord) * float4(input.Color, 1.0);
    
    float3 N = normalize(input.Normal);
    float3 L = normalize(lights[0].position - input.WorldPosition);
    float3 V = normalize(-input.WorldPosition);
    float3 R = normalize(-reflect(L, N));
    float3 diffuse = max(dot(N, L), 0.1) * color;
    
    color = float4(diffuse, 1.0);
    
    RayDesc ray;
    ray.Origin = input.WorldPosition;
    ray.Direction = L;
    ray.TMin = camera.nearPlane;
    ray.TMax = camera.farPlane;
        
    RayQuery < RAY_FLAG_ACCEPT_FIRST_HIT_AND_END_SEARCH > rayQuery;
    rayQuery.TraceRayInline(as, 0, 0xff, ray);
    
    while (rayQuery.Proceed())
    {
        uint candidateType = rayQuery.CandidateType();
        
        if (candidateType == CANDIDATE_NON_OPAQUE_TRIANGLE)
        {
            rayQuery.CommitNonOpaqueTriangleHit();
        }
    }

    if (rayQuery.CommittedStatus() == COMMITTED_TRIANGLE_HIT)
    {
        color.rgb *= 0.1;
    }
    
    return color;
}
