struct Payload
{
    float4 color;
};

RaytracingAccelerationStructure rs : register(t0, space0);
RWTexture2D<float4> outputTexture : register(u1, space0);

float rangeMap(float value, float min, float max, float newMin, float newMax)
{
    return newMin + (value - min) * (newMax - newMin) / (max - min);
}

[shader("raygeneration")]
void rayGen()
{
    const float fov = 45.0;
    const float3 position = float3(0, 0.0, -2.0);
    const float3 forward = normalize(float3(0, 0, 1));
    const float3 right = normalize(cross(float3(0, 1, 0), forward));
    const float3 up = cross(forward, right);
    
    uint3 LaunchID = DispatchRaysIndex();
    uint3 LaunchSize = DispatchRaysDimensions();
    
    float x = LaunchID.x + 0.5;
    float y = LaunchID.y + 0.5;
    
    float scale = tan(radians(fov));
    float aspectRatio = LaunchSize.x / float(LaunchSize.y);
    
    x = rangeMap(x, 0, LaunchSize.x - 1, -1.0, 1.0);
    y = rangeMap(y, 0, LaunchSize.y - 1, 1.0, -1.0);
    
    float3 direction = normalize(forward + x * right + y * up);
    
    RayDesc rayDesc;
    rayDesc.Origin = position;
    rayDesc.Direction = direction;
    rayDesc.TMin = 0.001;
    rayDesc.TMax = 100.0;
    
    Payload payload;
    TraceRay(rs, RAY_FLAG_FORCE_OPAQUE, 0xff, 0, 0, 0, rayDesc, payload);
    
    outputTexture[LaunchID.xy] = payload.color;

}
