struct CameraProperties
{
    float4x4 viewInverse;
    
    float4x4 projInverse;
};

struct Payload
{
    float4 color;
};

RaytracingAccelerationStructure rs : register(t0, space0);
RWTexture2D<float4> outputTexture : register(u1, space0);
ConstantBuffer<CameraProperties> cameraProperties : register(b2, space0);

[shader("raygeneration")]
void rayGen()
{
    uint3 LaunchID = DispatchRaysIndex();
    uint3 LaunchSize = DispatchRaysDimensions();

    const float2 pixelCenter = float2(LaunchID.xy) + float2(0.5, 0.5);
    const float2 inUV = pixelCenter / float2(LaunchSize.xy);
    float2 d = inUV * 2.0 - 1.0;
    float4 target = mul(cameraProperties.projInverse, float4(d.x, d.y, 1, 1));
    
    RayDesc rayDesc;
    rayDesc.Origin = mul(cameraProperties.viewInverse, float4(0, 0, 0, 1)).xyz;
    rayDesc.Direction = mul(cameraProperties.viewInverse, float4(normalize(target.xyz), 0)).xyz;
    rayDesc.TMin = 0.001;
    rayDesc.TMax = 10000.0;
    
    Payload payload;
    TraceRay(rs, RAY_FLAG_FORCE_OPAQUE, 0xff, 0, 0, 0, rayDesc, payload);
    
    outputTexture[LaunchID.xy] = 1.0;
}
