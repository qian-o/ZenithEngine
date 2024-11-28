struct VSInput
{
    float3 position : POSITION;
    
    float4 color : COLOR;
};

struct PSInput
{
    float4 position : SV_POSITION;
    
    float4 color : COLOR;
};

PSInput VSMain(VSInput input)
{
    PSInput output = (PSInput) 0;
    
    // Error code.
    output.Position = float4(input.position, 1.0f);
    output.Color = input.color;
    
    return output;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    return input.color;
}