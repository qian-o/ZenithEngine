struct Constants
{
    float4x4 Projection;
};

struct VSInput
{
    float2 Position : POSITION0;
    
    float2 UV : TEXCOORD0;
    
    float4 Color : COLOR0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    
    float2 UV : TEXCOORD0;
    
    float4 Color : COLOR0;
};

ConstantBuffer<Constants> constants : register(b0, space0);
SamplerState sampler0 : register(s0, space0);
Texture2D texture0 : register(t0, space1);

float3 SrgbToLinear(float3 srgb)
{
    return srgb * (srgb * (srgb * 0.305306011f + 0.682171111f) + 0.012522878f);
}

VSOutput VSMain(VSInput input)
{
    VSOutput output;
    
    output.Position = mul(float4(input.Position, 0.0f, 1.0f), constants.Projection);
    output.UV = input.UV;
    output.Color = input.Color;
    
#if 0
    output.Color.rgb = SrgbToLinear(output.Color.rgb);
#endif
    
    return output;
}

float4 PSMain(VSOutput input) : SV_TARGET
{
    return input.Color * texture0.Sample(sampler0, input.UV);
}