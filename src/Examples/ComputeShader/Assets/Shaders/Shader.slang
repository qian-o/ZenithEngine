// https://www.shadertoy.com/view/DlVczV
// This is an example from Shadertoy, used here to demonstrate the use of Compute Shader.

struct Constants
{
    float2 Resolution;

    float TotalTime;
};

ConstantBuffer<Constants> constants : register(b0, space0);
RWTexture2D<float4> Output : register(u0, space0);

float mod(float x, float y)
{
    return x - y * floor(x / y);
}

[numthreads(32, 32, 1)]
void Main(uint3 id: SV_DispatchThreadID)
{
    float4 color = float4(0.0, 0.0, 0.0, 0.0);

    float2 v = constants.Resolution;
    float2 p = (float2(id.xy) * 2.0 - v) / v.y;

    for (float i = 0.2, l; i < 1.0; i += 0.05)
    {
        color += (cos(i * 5.0 + float4(0.0, 1.0, 2.0, 3.0)) + 1.0) * (1.0 + v.y / (l = length(v) + 0.003)) / l;
        v = float2(mod(atan2(p.x, p.y) + i + i * constants.TotalTime, 6.28318530718) - 3.14159265359, 1.0) * length(p) -
            i;
        v.x -= clamp(v.x += i, -i, i);
    }

    color = tanh(color / 1e2);

    Output[id.xy] = color;
}
