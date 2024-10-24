#include "RayQuery.hlsl"

float4 main(VSOutput input) : SV_TARGET
{
    GeometryNode node = geometryNodes[input.NodeIndex];

    float4 texColor =
        textureArray[node.baseColorTextureIndex].Sample(samplerArray[0], input.TexCoord) * float4(input.Color, 1.0);

    if (node.alphaMask && texColor.a < node.alphaCutoff)
    {
        discard;
    }

    float3 N = normalize(input.Normal);
    float3 L = normalize(lights[0].position - input.WorldPosition);
    float3 V = normalize(-input.WorldPosition);
    float3 R = normalize(-reflect(L, N));
    float3 diffuse = max(dot(N, L), 0.1) * texColor.rgb;

    float3 finalColor = diffuse;

    uint2 launchDim = uint2(param.width, param.height);
    uint2 launchIndex = input.Position.xy;

    uint randSeed = initRand(launchIndex.x + launchIndex.y * launchDim.x, 0, 16);

    float3 coneSample = getConeSample(randSeed, input.WorldPosition, lights[0].position, 0.2);

    RayQuery < RAY_FLAG_ACCEPT_FIRST_HIT_AND_END_SEARCH > rayQuery;
    ProceedRayQuery(rayQuery, input.WorldPosition, coneSample, camera.nearPlane, camera.farPlane);

    if (rayQuery.CommittedStatus() == COMMITTED_TRIANGLE_HIT)
    {
        finalColor *= 0.5;
    }

    float ambientOcclusion = 0.0f;
    for (int i = 0; i < 4; i++)
    {
        float3 aoDir = getCosHemisphereSample(randSeed, N);

        ProceedRayQuery(rayQuery, input.WorldPosition, aoDir, 0.01, 1.2);

        if (rayQuery.CommittedStatus() != COMMITTED_TRIANGLE_HIT)
        {
            ambientOcclusion += 1.0;
        }
    }

    float aoColor = ambientOcclusion / 4;
    finalColor *= aoColor;

    return float4(finalColor, 1);
}
