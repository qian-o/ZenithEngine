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