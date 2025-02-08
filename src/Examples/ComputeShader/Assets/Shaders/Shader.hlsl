struct Constants
{
    uint2 Resolution;

    float DeltaTime;

    float TotalTime;
};

ConstantBuffer<Constants> constants : register(b0, space0);
RWTexture2D<float4> Output : register(u0, space0);

[numthreads(8, 8, 1)]
void CSMain(uint3 id : SV_DispatchThreadID)
{
    float2 uv = float2(id.xy) / float2(constants.Resolution);

    Output[id.xy] = float4(uv, 0, 1);
}