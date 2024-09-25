struct Camera
{
    float3 position;
    
    float3 forward;
    
    float3 right;
    
    float3 up;
    
    float nearPlane;
    
    float farPlane;
    
    float fov;
    
    uint width;
    
    uint height;
    
    uint antiAliasing;
    
    uint maxSteps;
    
    float epsilon;
};

ConstantBuffer<Camera> camera : register(b0, space0);
RWTexture2D<float4> outputTexture : register(u1, space0);

float rangeMap(float value, float min, float max, float newMin, float newMax)
{
    return newMin + (value - min) * (newMax - newMin) / (max - min);
}

float3 render(float2 pixel)
{
    float x = pixel.x;
    float y = pixel.y;
    
    float scale = tan(camera.fov);
    float aspectRatio = camera.width / camera.height;
    
    x = rangeMap(x, 0, camera.width - 1, -1.0, 1.0);
    y = rangeMap(y, 0, camera.height - 1, -1.0, 1.0);

    if (aspectRatio > 1.0)
    {
        x *= aspectRatio * scale;
        y *= scale;
    }
    else
    {
        x *= scale;
        y *= scale / aspectRatio;
    }
    
    float3 position = camera.position;
    float3 direction = normalize(camera.forward + x * camera.right + y * camera.up);
    
    return direction;
};

[numthreads(1, 1, 1)]
void main(uint3 id : SV_DispatchThreadID)
{
    float3 color = float3(0);
    
    float halfAA = camera.antiAliasing / 2.0;
    
    for (uint i = 0; i < camera.antiAliasing; i++)
    {
        for (uint j = 0; j < camera.antiAliasing; j++)
        {
            float2 offset = float2(i, j) / halfAA;
            
            float2 pixel = float2(id.x, id.y) + offset;
            
            color += render(pixel);
        }
    }
    
    color /= camera.antiAliasing * camera.antiAliasing;
    
    outputTexture[id.xy] = float4(pow(color, 1.0 / 2.2), 1);
}