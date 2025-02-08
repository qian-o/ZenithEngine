#define NUM_AA 8
#define NUM_FOV 0.785398163
#define NUM_MIN 0.1
#define NUM_MAX 100
#define NUM_STEPS 64

struct Constants
{
    float2 Resolution;

    float DeltaTime;

    float TotalTime;
};

ConstantBuffer<Constants> constants : register(b0, space0);
RWTexture2D<float4> Output : register(u0, space0);

float3 RotateY(float3 p, float a)
{
    float c = cos(a);
    float s = sin(a);

    return float3(p.x * c + p.z * s, p.y, -p.x * s + p.z * c);
}

float2 OpU(float2 d1, float2 d2)
{
    return (d1.x < d2.x) ? d1 : d2;
}

float Box(float3 p, float3 b)
{
    float3 d = abs(p) - b;

    return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
}

float Map(float3 position)
{
    float d = 1.0;
    float3 p = position;

    p = RotateY(p, constants.TotalTime);
    d = OpU(d, float2(Box(p, float3(0.5, 0.5, 0.5)), 1));

    return d;
}

float2 TraceRay(float3 position, float3 direction)
{
    float min = NUM_MIN;
    float max = NUM_MAX;

    float2 result = float2(-1, -1);

    for (uint i = 0; i < NUM_STEPS; i++)
    {
        if (position.z > max)
        {
            break;
        }

        float hit = Map(position);
        
        if (hit < 0.01)
        {
            result = hit;

            break;
        }

        position += direction * hit.x;
    }

    return result;
}

float3 RayMarching(float2 xy)
{
    float2 uv = xy / constants.Resolution;
    uv = uv * 2.0 - 1.0;

    float aspect = constants.Resolution.x / constants.Resolution.y;

    if (aspect > 1.0)
    {
        uv.x *= aspect * NUM_FOV;
        uv.y *= NUM_FOV;
    }
    else
    {
        uv.x *= NUM_FOV;
        uv.y *= NUM_FOV / aspect;
    }

    float3 position = float3(0, 0, -10);
    float3 direction = normalize(float3(uv, 1));

    float2 result = TraceRay(position, direction);

    if (result.y < 0.0)
    {
        return float3(0, 0, 0);
    }

    return direction;
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