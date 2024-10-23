struct GeometryNode
{
    float4x4 transform;

    bool alphaMask;

    float alphaCutoff;

    bool doubleSided;

    float4 baseColorFactor;

    uint baseColorTextureIndex;

    uint normalTextureIndex;

    uint roughnessTextureIndex;
};

struct Camera
{
    float3 position;

    float3 forward;

    float3 right;

    float3 up;

    float nearPlane;

    float farPlane;

    float fov;

    float4x4 view;

    float4x4 projection;
};

struct Light
{
    float3 position;

    float4 ambientColor;

    float4 diffuseColor;
};

struct VSInput
{
    [[vk::location(0)]]
    float3 Position : POSITION0;

    [[vk::location(1)]]
    float3 Normal : NORMAL0;

    [[vk::location(2)]]
    float2 TexCoord : TEXCOORD0;

    [[vk::location(3)]]
    float3 Color : COLOR0;

    [[vk::location(4)]]
    float4 Tangent : TEXCOORD1;

    [[vk::location(5)]]
    uint NodeIndex : TEXCOORD2;
};

struct VSOutput
{
    float4 Position : SV_POSITION;

    [[vk::location(0)]]
    float3 WorldPosition : POSITION0;

    [[vk::location(1)]]
    float3 Normal : NORMAL0;

    [[vk::location(2)]]
    float2 TexCoord : TEXCOORD0;

    [[vk::location(3)]]
    float3 Color : COLOR0;

    [[vk::location(4)]]
    float4 Tangent : TEXCOORD1;

    [[vk::location(5)]]
    nointerpolation uint NodeIndex : TEXCOORD2;
};

RaytracingAccelerationStructure as : register(t0, space0);
StructuredBuffer<GeometryNode> geometryNodes : register(t1, space0);
ConstantBuffer<Camera> camera : register(b2, space0);
StructuredBuffer<Light> lights : register(t3, space0);
Texture2D textureArray[] : register(t0, space1);
SamplerState samplerArray[] : register(s0, space2);

VSOutput mainVS(VSInput input)
{
    GeometryNode node = geometryNodes[input.NodeIndex];

    float4 worldPosition = mul(node.transform, float4(input.Position, 1.0));

    VSOutput output;
    output.Position = mul(camera.projection, mul(camera.view, worldPosition));
    output.WorldPosition = worldPosition.xyz;
    output.Normal = normalize(mul(node.transform, float4(input.Normal, 0.0)).xyz);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    output.Tangent = input.Tangent;
    output.NodeIndex = input.NodeIndex;

    return output;
}

template <uint Flags>
void ProceedRayQuery(RayQuery<Flags> rayQuery, float3 origin, float3 direction, float tmin, float tmax)
{
    RayDesc ray;
    ray.Origin = origin;
    ray.Direction = direction;
    ray.TMin = tmin;
    ray.TMax = tmax;

    rayQuery.TraceRayInline(as, 0, 0xff, ray);

    while (rayQuery.Proceed())
    {
        uint candidateType = rayQuery.CandidateType();

        if (candidateType == CANDIDATE_NON_OPAQUE_TRIANGLE)
        {
            rayQuery.CommitNonOpaqueTriangleHit();
        }
    }
}

float calculateAmbientOcclusion(float3 object_point, float3 object_normal)
{
    const float ao_mult = 1;
    uint max_ao_each = 3;
    uint max_ao = max_ao_each * max_ao_each;
    const float max_dist = 2;
    const float tmin = 0.01, tmax = max_dist;
    float accumulated_ao = 0.f;
    float3 u = abs(dot(object_normal, float3(0, 0, 1))) > 0.9 ? cross(object_normal, float3(1, 0, 0))
                                                              : cross(object_normal, float3(0, 0, 1));
    float3 v = cross(object_normal, u);
    float accumulated_factor = 0;
    for (uint j = 0; j < max_ao_each; ++j)
    {
        float phi = 0.5 * (-3.14159 + 2 * 3.14159 * (float(j + 1) / float(max_ao_each + 2)));
        for (uint k = 0; k < max_ao_each; ++k)
        {
            float theta = 0.5 * (-3.14159 + 2 * 3.14159 * (float(k + 1) / float(max_ao_each + 2)));
            float x = cos(phi) * sin(theta);
            float y = sin(phi) * sin(theta);
            float z = cos(theta);
            float3 direction = x * u + y * v + z * object_normal;

            RayQuery<RAY_FLAG_ACCEPT_FIRST_HIT_AND_END_SEARCH> rayQuery;
            ProceedRayQuery(rayQuery, object_point, direction, tmin, tmax);

            float dist = max_dist;
            if (rayQuery.CommittedStatus() != COMMITTED_NOTHING)
            {
                dist = rayQuery.CommittedRayT();
            }

            float ao = min(dist, max_dist);
            float factor = 0.2 + 0.8 * z * z;

            accumulated_factor += factor;
            accumulated_ao += ao * factor;
        }
    }

    accumulated_ao /= (max_dist * accumulated_factor);
    accumulated_ao *= accumulated_ao;
    accumulated_ao = max(min((accumulated_ao)*ao_mult, 1), 0);

    return accumulated_ao;
}

float4 mainPS(VSOutput input) : SV_TARGET
{
    GeometryNode node = geometryNodes[input.NodeIndex];

    float4 color =
        textureArray[node.baseColorTextureIndex].Sample(samplerArray[0], input.TexCoord) * float4(input.Color, 1.0);

    float3 N = normalize(input.Normal);
    float3 L = normalize(lights[0].position - input.WorldPosition);
    float3 V = normalize(-input.WorldPosition);
    float3 R = normalize(-reflect(L, N));
    float3 diffuse = max(dot(N, L), 0.1) * color;

    color = float4(diffuse, 1.0);

    RayQuery<RAY_FLAG_ACCEPT_FIRST_HIT_AND_END_SEARCH> rayQuery;
    ProceedRayQuery(rayQuery, input.WorldPosition, L, camera.nearPlane, camera.farPlane);

    if (rayQuery.CommittedStatus() == COMMITTED_TRIANGLE_HIT)
    {
        color.rgb *= 0.3;
    }

    color.rgb *= calculateAmbientOcclusion(input.WorldPosition, N);

    return color;
}
