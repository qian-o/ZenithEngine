import "Constants";

public enum LightType : int
{
    Unknown = 0,

    Area = 1,

    Sphere = 2
}

public struct Vertex
{
    public float3 Position;

    public float3 Normal;

    public float2 TexCoord;

    public int MaterialIndex;

    public static Vertex GetVertex(in Uniforms uniforms, BuiltInTriangleIntersectionAttributes attrib)
    {
        uint2 offset = uniforms.Offsets[GeometryIndex()];

        uint primitiveIndex = PrimitiveIndex();

        Vertex v0 = uniforms.Vertices[offset.x + uniforms.Indices[offset.y + (primitiveIndex * 3)]];
        Vertex v1 = uniforms.Vertices[offset.x + uniforms.Indices[offset.y + (primitiveIndex * 3 + 1)]];
        Vertex v2 = uniforms.Vertices[offset.x + uniforms.Indices[offset.y + (primitiveIndex * 3 + 2)]];

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

    public float Sheen;

    public float SheenTint;

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
        rayDesc.TMin = Constants.NEAR;
        rayDesc.TMax = Constants.FAR;

        return rayDesc;
    }
}

public struct Globals
{
    public Camera Camera;

    public bool DoubleSidedLighting;

    public uint SampleCount;

    public uint MaxDepth;

    public uint FrameIndex;

    public bool UseHdr;

    public float HdrMultiplier;

    public float HdrResolution;
}

public struct Uniforms
{
    public RaytracingAccelerationStructure Scene;

    public StructuredBuffer<Material> Materials;

    public StructuredBuffer<Vertex> Vertices;

    public StructuredBuffer<uint> Indices;

    public StructuredBuffer<uint2> Offsets;

    public Globals Globals;

    public Texture2D Textures[128];

    public SamplerState Samplers[128];

    public Texture2D HdrTextures[3];

    public SamplerState HdrSamplers[3];

    public StructuredBuffer<Light> Lights;

    public RWTexture2D Accumulation;

    public RWTexture2D Output;
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
    public uint Seed;

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

public struct ShadowPayload
{
    public bool InShadow = true;
};