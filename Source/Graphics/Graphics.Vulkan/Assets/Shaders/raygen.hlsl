struct CameraProperties
{
    float4x4 inverseView;
    float4x4 inverseProjection;
};

struct RayPayload
{
    float4 color;
};

RaytracingAccelerationStructure rs : register(t0, space0);
RWTexture2D<float4> image : register(u1, space0);
ConstantBuffer<CameraProperties> cam : register(b0, space0);

[shader("raygeneration")]
void main()
{
    uint3 dispatchRaysIndex = DispatchRaysIndex();
    uint3 dispatchRaysDimensions = DispatchRaysDimensions();
    
    const float2 pixelCenter = (float2(dispatchRaysIndex.xy) + 0.5);
    const float2 screenUV = pixelCenter / float2(dispatchRaysDimensions.xy);
    
    float2 direction = screenUV * 2.0 - 1.0;
    float4 target = mul(cam.inverseProjection, float4(direction, 1.0, 1.0));
    
    RayDesc ray;
    ray.Origin = mul(cam.inverseView, float4(0.0, 0.0, 0.0, 1.0)).xyz;
    ray.Direction = mul(cam.inverseView, float4(normalize(target.xyz), 0)).xyz;
    ray.TMin = 0.001;
    ray.TMax = 1000.0;
    
    RayPayload payload;
    TraceRay(rs, 0x0, 0xff, 0, 0, 0, ray, payload);
    
    image[dispatchRaysIndex.xy] = payload.color;
}