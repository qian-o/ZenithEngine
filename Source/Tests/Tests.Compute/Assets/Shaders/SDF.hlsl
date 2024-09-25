struct DispatchSize
{
    uint x;
    uint y;
};

ConstantBuffer<DispatchSize> dispatchSize : register(b0, space0);
RWTexture2D<float4> outputTexture : register(u1, space0);

[numthreads(1, 1, 1)]
void main(uint3 id : SV_DispatchThreadID)
{
    // Get the current pixel position
    float2 uv = id.xy / float2(dispatchSize.x, dispatchSize.y);
    float2 center = float2(0.5, 0.5);
    float2 p = uv - center;

    // Compute the distance to the circle
    float d = length(p) - 0.3;

    // Compute the color
    float4 color = float4(1, 1, 1, 1);
    if (d < 0)
    {
        color = float4(1, 0, 0, 1);
    }

    // Write the color to the output texture
    outputTexture[id.xy] = color;
}