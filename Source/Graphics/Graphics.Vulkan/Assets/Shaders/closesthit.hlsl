struct RayPayload
{
    float4 color;
};

[shader("closesthit")]
void main(inout RayPayload payload, BuiltInTriangleIntersectionAttributes attribs)
{
    const float3 barycentrics = float3(1.0 - attribs.barycentrics.x - attribs.barycentrics.y, attribs.barycentrics.x, attribs.barycentrics.y);
    
    payload.color = float4(barycentrics, 1.0);
}