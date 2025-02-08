#define PI 3.1415926538

struct Constants
{
    float2 Resolution;

    float DeltaTime;

    float TotalTime;
};

ConstantBuffer<Constants> constants : register(b0, space0);
RWTexture2D<float4> Output : register(u0, space0);

float2 Rotate(float2 uv, float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    
    return float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);
}

float3 HSVToRGB(float3 hsv)
{
    float3 rgb = clamp(abs(fmod(hsv.x * 6.0 + float3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);

    rgb = rgb * rgb * (3.0 - 2.0 * rgb);

    return lerp(1.0, lerp(1.0, rgb, hsv.y), hsv.z);
}

[numthreads(8, 8, 1)]
void CSMain(uint3 id : SV_DispatchThreadID)
{
    float2 uv = (float2(id.xy) + 0.5) / constants.Resolution;
    uv = uv * 2.0 - 1.0;

    float aspectRatio = constants.Resolution.x / constants.Resolution.y;
    
    if (aspectRatio > 1.0)
    {
        uv.x *= aspectRatio;
    }
    else
    {
        uv.y /= aspectRatio;
    }

    uv = Rotate(uv, constants.TotalTime);

    float len = length(uv);

    if (len > 1.0)
    {
        Output[id.xy] = float4(0, 0, 0, 1);

        return;
    }

    float angle = dot(normalize(uv), float2(1.0, 0.0));
    angle = acos(angle);
    
    if (uv.y < 0.0)
    {
        angle = 2.0 * PI - angle;
    }

    Output[id.xy] = float4(HSVToRGB(float3(angle / (2.0 * PI), len, 1.0)), 1.0);
}