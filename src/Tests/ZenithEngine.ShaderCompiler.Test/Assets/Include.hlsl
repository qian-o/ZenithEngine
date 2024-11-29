#include "Common.hlsl"

PSInput VSMain(VSInput input)
{
    PSInput output = (PSInput) 0;
    
    output.position = float4(input.position, 1.0f);
    output.color = input.color;
    
    return output;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    return input.color;
}