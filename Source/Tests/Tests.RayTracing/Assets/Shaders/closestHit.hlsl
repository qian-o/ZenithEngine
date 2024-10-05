struct Payload
{
    float4 color;
};

[shader("closesthit")]
void closestHit(inout Payload payload, in BuiltInTriangleIntersectionAttributes attribs)
{
    const float3 barycentricCoords = float3(1.0f - attribs.barycentrics.x - attribs.barycentrics.y, attribs.barycentrics.x, attribs.barycentrics.y);
    
    payload.color = float4(barycentricCoords, 1.0);
}
