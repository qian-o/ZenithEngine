#define NUM_AA 8

struct Constants
{
    uint2 Resolution;

    float DeltaTime;

    float TotalTime;
};

ConstantBuffer<Constants> constants : register(b0, space0);
RWTexture2D<float4> Output : register(u0, space0);

float3 RayMarching(float2 xy)
{
    float2 uv = xy / float2(constants.Resolution);
    uv = uv * 2.0 - 1.0;

    return float3(uv, 0);
}

[numthreads(8, 8, 1)]
void CSMain(uint3 id : SV_DispatchThreadID)
{
    if (NUM_AA == 1)
    {
        Output[id.xy] = float4(RayMarching(float2(id.xy)), 1);
    }
    else
    {
        const float halfAA = float(NUM_AA) * 0.5;

        float2 xy = float2(id.xy);

        float3 color = 0;
        for (uint i = 0; i < NUM_AA; i++)
        {
            for (uint j = 0; j < NUM_AA; j++)
            {
                float2 offset = float2(i, j) / halfAA;

                color += RayMarching(xy + offset);
            }
        }
        color /= float(NUM_AA * NUM_AA);

        Output[id.xy] = float4(color, 1);
    }
}