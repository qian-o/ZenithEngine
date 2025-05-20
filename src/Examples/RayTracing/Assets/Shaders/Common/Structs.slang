public const float PI = 3.14159265358979323846;
public const float TWO_PI = 6.28318530717958647692;
public const float INV_PI = 0.31830988618379067154;
public const float INV_2PI = 0.15915494309189533577;

public const float EPS = 0.001;
public const float NEAR = 0.0001;
public const float FAR = 1000000.0;

public enum LightType : int
{
    Area = 0,

    Sphere = 1
}

public struct Vertex
{
    public float3 Position;

    public float3 Normal;

    public float2 TexCoord;

    public int MaterialIndex;

    public static Vertex GetVertex(StructuredBuffer<Vertex> vertices, StructuredBuffer<uint> indices,
                                   StructuredBuffer<uint2> offsets, BuiltInTriangleIntersectionAttributes attrib)
    {
        uint2 offset = offsets[GeometryIndex()];

        uint primitiveIndex = PrimitiveIndex();

        Vertex v0 = vertices[offset.x + indices[offset.y + (primitiveIndex * 3)]];
        Vertex v1 = vertices[offset.x + indices[offset.y + (primitiveIndex * 3 + 1)]];
        Vertex v2 = vertices[offset.x + indices[offset.y + (primitiveIndex * 3 + 2)]];

        float3 barycentrics = float3(1.0 - attrib.barycentrics.x - attrib.barycentrics.y, attrib.barycentrics.xy);

        Vertex result;
        result.Position = barycentrics.x * v0.Position + barycentrics.y * v1.Position + barycentrics.z * v2.Position;
        result.Normal = normalize(barycentrics.x * v0.Normal + barycentrics.y * v1.Normal + barycentrics.z * v2.Normal);
        result.TexCoord = barycentrics.x * v0.TexCoord + barycentrics.y * v1.TexCoord + barycentrics.z * v2.TexCoord;
        result.MaterialIndex = (int)(barycentrics.x * v0.MaterialIndex + barycentrics.y * v1.MaterialIndex +
                                     barycentrics.z * v2.MaterialIndex);

        return result;
    }
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

    public int MetallicRoughnessTextureIndex;

    public int NormalTextureIndex;

    public int HeightTextureIndex;
}

public struct Light
{
    public LightType Type;

    public float3 Position;

    public float3 Emission;

    public float3 U;

    public float3 V;

    public float Area;

    public float Radius;
}

public struct Camera
{
    public float3 Position;

    public float3 Forward;

    public float3 Right;

    public float3 Up;

    public float Fov;

    public RayDesc GetRayDesc(float2 offset)
    {
        uint2 launchID = DispatchRaysIndex().xy;
        uint2 launchSize = DispatchRaysDimensions().xy;

        float aspectRatio = (float)launchSize.x / launchSize.y;

        float2 xy = ((launchID + offset) / launchSize * 2.0 - 1.0) * tan(Fov * 0.5);

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
        rayDesc.TMin = NEAR;
        rayDesc.TMax = FAR;

        return rayDesc;
    }
}

public struct Globals
{
    public Camera Camera;

    public uint LightCount;

    public bool DoubleSidedLighting;

    public uint SampleCount;

    public uint MaxDepth;

    public uint FrameIndex;

    public bool UseHdr;

    public float HdrMultiplier;

    public float HdrResolution;
}

public struct Ray
{
    public float3 Origin;

    public float3 Direction;
}

public struct LightSample
{
    public float3 Normal;

    public float3 Position;

    public float3 Emission;

    public float Pdf;
}

public struct BSDFSample
{
    public float3 Direction;

    public float Pdf;
};

public struct Payload
{
    public Ray Ray;

    public BSDFSample Bsdf;

    public float3 Radiance;

    public float3 Absorbtion;

    public float3 Beta = float3(1.0, 1.0, 1.0);

    public float3 Position;

    public float3 Normal;

    public float3 ForwardNormal;

    public uint Depth;

    public bool Stop;

    public float Eta;
};