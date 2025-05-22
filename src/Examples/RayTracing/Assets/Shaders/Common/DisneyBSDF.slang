import "Structs";

public class DisneyBSDF
{
    public static float3 Eval(in Payload payload, in Material material, in float3 L, inout float pdf)
    {
        float3 N = payload.ForwardNormal;
        float3 V = -WorldRayDirection();
        float eta = payload.Eta;
        bool refl = dot(N, L) > 0.0;

        float3 H = refl ? normalize(L + V) : normalize(L + eta * V);

        if (dot(V, H) <= 0.0)
        {
            H = -H;
        }

        float diffuseRatio = 0.5 * (1.0 - material.Metallic);
        float primarySpecRatio = 1.0 / (1.0 + material.ClearCoat);
        float transWeight = (1.0 - material.Metallic) * material.Transmission;

        float3 brdf = float3(0.0, 0.0, 0.0);
        float3 bsdf = float3(0.0, 0.0, 0.0);
        float brdfPdf = 0.0;
        float bsdfPdf = 0.0;

        if (transWeight > 0.0)
        {
            if (refl)
            {
                // Reflection
                bsdf = EvalDielectricReflection(material, V, N, L, H, bsdfPdf);
            }
            else
            { 
                // Transmission
                bsdf = EvalDielectricRefraction(material, V, N, L, H, bsdfPdf);
            }
        }
    }

    private static float3 EvalDielectricReflection(in Material material, in float3 V, in float3 N, in float3 L, in float3 H, out float pdf)
    {
    }

    private static float3 EvalDielectricRefraction(in Material material, in float3 V, in float3 N, in float3 L, in float3 H, out float pdf)
    {
    }
}
