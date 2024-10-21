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
    [[vk::location(0)]] float3 Normal : NORMAL0;
    [[vk::location(1)]] float2 TexCoord : TEXCOORD0;
    [[vk::location(2)]] float3 Color : COLOR0;
    [[vk::location(3)]] float4 Tangent : TEXCOORD1;
    [[vk::location(4)]] nointerpolation uint NodeIndex : TEXCOORD2;
};

RaytracingAccelerationStructure as : register(t0, space0);
StructuredBuffer<GeometryNode> geometryNodes : register(t1, space0);
ConstantBuffer<Camera> camera : register(b2, space0);
Texture2D textureArray[] : register(t0, space1);
SamplerState samplerArray[] : register(s0, space2);

VSOutput mainVS(VSInput input)
{
    GeometryNode node = geometryNodes[input.NodeIndex];
    
    VSOutput output;
    output.Position = mul(camera.projection, mul(camera.view, mul(node.transform, float4(input.Position, 1.0))));
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
    
    float4 baseColor = textureArray[node.baseColorTextureIndex].Sample(samplerArray[0], input.TexCoord);
    
    return baseColor;
}