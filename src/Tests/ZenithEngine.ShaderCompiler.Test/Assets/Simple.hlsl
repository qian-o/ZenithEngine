struct VSInput
{
    float3 position : POSITION;
    
    float4 color : COLOR;
    
    float2 texCoord : TEXCOORD;
};

struct PSInput
{
    float4 position : SV_POSITION;
    
    float4 color : COLOR;
    
    float2 texCoord : TEXCOORD;
};

struct MVP
{
    float4x4 model;
    
    float4x4 view;
    
    float4x4 projection;
    
    float4x4 mvp;
};

ConstantBuffer<MVP> mvp1 : register(b0);
StructuredBuffer<float4x4> mvp2 : register(t0);
Texture2D texture1[10] : register(t0, space1);
Texture2D texture2[] : register(t1, space1);
SamplerState samplerState[] : register(s0, space2);

PSInput VSMain(VSInput input)
{
    PSInput output = (PSInput) 0;
    
    output.position = mul(mvp2[0], mul(mvp1.mvp, float4(input.position, 1.0)));
    output.color = input.color;
    output.texCoord = input.texCoord;
    
    return output;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    float4 color = texture1[0].Sample(samplerState[0], input.texCoord);
    color *= texture2[0].Sample(samplerState[0], input.texCoord);
    
    return input.color * color;
}