public struct Vertex
{
    public float3 Position;

    public float3 Normal;

    public float2 TexCoord;
}

public struct Material
{
    public float4 Albedo;

    public float4 Emission;

    public float4 Extinction;

    public float Metallic;

    public float Roughness;

    public float SubSurface;

    public float SpecularTint;

    public float ClearCoat;

    public float ClearCoatGloss;

    public float Transmission;

    public float IOR;

    public float AttenuationDistance;

    public int AlbedoTextureIndex;

    public int EemissionTextureIndex;

    public int NormalTextureIndex;

    public int HeightTextureIndex;
}

public struct Camera
{
    public float3 Position;

    public float3 Forward;

    public float3 Right;

    public float3 Up;

    public float NearPlane;

    public float FarPlane;

    public float Fov;

    public RayDesc GetRayDesc()
    {
        uint2 launchID = DispatchRaysIndex().xy;
        uint2 launchSize = DispatchRaysDimensions().xy;

        float aspectRatio = (float)launchSize.x / launchSize.y;

        float2 xy = (launchID / launchSize * 2.0 - 1.0) * tan(Fov * 0.5);

        if (aspectRatio > 1.0)
        {
            xy.x *= aspectRatio;
        }
        else
        {
            xy.y /= aspectRatio;
        }

        xy.y = -xy.y;

        RayDesc rayDesc;
        rayDesc.Origin = Position;
        rayDesc.Direction = normalize(Forward + xy.x * Right + xy.y * Up);
        rayDesc.TMin = NearPlane;
        rayDesc.TMax = FarPlane;

        return rayDesc;
    }
}

public struct Globals
{
    public Camera Camera;
}
