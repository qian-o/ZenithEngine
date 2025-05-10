struct VertexInput
{
    float3 Position : POSITION;

    float3 Normal : NORMAL;

    float2 TexCoord : TEXCOORD0;
};

struct VertexOutput
{
    float4 Position : SV_POSITION;

    float3 Normal : NORMAL;

    float2 TexCoord : TEXCOORD0;
};

struct Constants
{
    float4x4 Model;
};

uniform Constants constants;

VertexOutput VertexMain(VertexInput input)
{
    VertexOutput output;
    output.Position = mul(constants.Model, float4(input.Position, 1.0));
    output.Normal = input.Normal;
    output.TexCoord = input.TexCoord;

    return output;
}

float4 PixelMain(VertexOutput input) : SV_TARGET
{
    return float4(input.TexCoord, 1.0, 1.0);
}
