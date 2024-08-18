struct UBO
{
    float4x4 Model;
    float4x4 View;
    float4x4 Projection;
};

struct Properties
{
    float DistanceMark;
    float SmoothDelta;
};

struct VSInput
{
    [[vk::location(0)]] float3 Position : POSITION0;
    [[vk::location(1)]] float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    [[vk::location(0)]] float2 TexCoord : TEXCOORD;
};

ConstantBuffer<UBO> ubo : register(b0, space0);
ConstantBuffer<Properties> properties : register(b1, space0);
SamplerState textureSampler : register(s2, space0);
Texture2D textureSDF : register(t0, space1);

VSOutput mainVS(VSInput input)
{
    VSOutput output;
    
    output.Position = mul(ubo.Projection, mul(ubo.View, mul(ubo.Model, float4(input.Position, 1.0))));
    output.TexCoord = input.TexCoord;
    
    return output;
}

float4 mainPS(VSOutput input) : SV_TARGET
{
    float4 sdf = textureSDF.Sample(textureSampler, input.TexCoord);
    
    float alpha = smoothstep(properties.DistanceMark - properties.SmoothDelta, properties.DistanceMark + properties.SmoothDelta, sdf.a);
    float3 rgb = lerp(float3(0.0), float3(1.0), alpha);
    
    return float4(rgb, alpha);
}