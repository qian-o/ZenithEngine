[[vk::constant_id(0)]] const bool AlphaMask = false;
[[vk::constant_id(1)]] const float AlphaCutoff = 0.0;

struct PerObject
{
    float4x4 Model;
};

struct Frame
{
    float4x4 Projection;
    float4x4 View;
    float4 LightPos;
    float4 ViewPos;
};

struct VSInput
{
    [[vk::location(0)]] float3 Position : POSITION0;
    [[vk::location(1)]] float3 Normal : NORMAL0;
    [[vk::location(2)]] float2 TexCoord : TEXCOORD0;
    [[vk::location(3)]] float3 Color : COLOR0;
    [[vk::location(4)]] float4 Tangent : TEXCOORD1;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    [[vk::location(0)]] float3 Normal : NORMAL0;
    [[vk::location(1)]] float2 TexCoord : TEXCOORD;
    [[vk::location(2)]] float3 Color : COLOR0;
    [[vk::location(3)]] float3 ViewVec : TEXCOORD1;
    [[vk::location(4)]] float3 LightVec : TEXCOORD2;
    [[vk::location(5)]] float4 Tangent : TEXCOORD3;
};

ConstantBuffer<PerObject> perObject : register(b0, space0);
ConstantBuffer<Frame> frame : register(b1, space0);
Texture2D textureColorMap : register(t0, space1);
SamplerState textureSampler : register(s1, space1);
Texture2D textureNormalMap : register(t2, space1);
SamplerState normalSampler : register(s3, space1);

VSOutput mainVS(VSInput input)
{
    float4 position = mul(perObject.Model, float4(input.Position, 1.0));
    
    VSOutput output;
    
    output.Position = mul(frame.Projection, mul(frame.View, position));
    output.Normal = normalize(mul(perObject.Model, float4(input.Normal, 0.0)).xyz);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    output.ViewVec = frame.ViewPos.xyz - position.xyz;
    output.LightVec = frame.LightPos.xyz - position.xyz;
    output.Tangent = input.Tangent;
    
    return output;
}

float4 mainPS(VSOutput input) : SV_TARGET
{
    float4 color = textureColorMap.Sample(textureSampler, input.TexCoord) * float4(input.Color, 1.0);
    
    if (AlphaMask && color.a < AlphaCutoff)
        discard;
    
    float3 N = normalize(input.Normal);
    float3 T = normalize(input.Tangent.xyz);
    float3 B = cross(input.Normal, input.Tangent.xyz) * input.Tangent.w;
    float3x3 TBN = float3x3(T, B, N);
    N = mul(normalize(textureNormalMap.Sample(normalSampler, input.TexCoord).xyz * 2.0 - float3(1.0, 1.0, 1.0)), TBN);
    
    const float ambientStrength = 0.1;
    float3 L = normalize(input.LightVec);
    float3 V = normalize(input.ViewVec);
    float3 R = reflect(-L, N);
    float3 diffuse = max(dot(N, L), ambientStrength).rrr;
    float3 specular = pow(max(dot(R, V), 0.0), 32.0);
    
    return float4(color.rgb * diffuse + specular, color.a);
}