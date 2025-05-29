import "Constants";
import "Maths";
import "Random";
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
            // Reflection
            if (refl)
            {
                bsdf = EvalDielectricReflection(payload, material, V, N, L, H, bsdfPdf);
            }
            else // Transmission
            {
                bsdf = EvalDielectricRefraction(payload, material, V, N, L, H, bsdfPdf);
            }
        }

        float m_pdf;

        if (transWeight < 1.0)
        {
            float3 Cdlin = material.Albedo.xyz;
            float Cdlum = 0.3 * Cdlin.x + 0.6 * Cdlin.y + 0.1 * Cdlin.z; // luminance approx.

            float3 Ctint = Cdlum > 0.0 ? Cdlin / Cdlum : float3(1.0f); // normalize lum. to isolate hue+sat
            float3 Cspec0 = lerp(material.Albedo.w * 0.08 * lerp(float3(1.0, 1.0, 1.0), Ctint, material.SpecularTint),
                                 Cdlin, material.Metallic);
            float3 Csheen = lerp(float3(1.0, 1.0, 1.0), Ctint, material.SheenTint);

            // Diffuse
            brdf += EvalDiffuse(material, Csheen, V, N, L, H, m_pdf);
            brdfPdf += m_pdf * diffuseRatio;

            // Specular
            brdf += EvalSpecular(material, Cspec0, V, N, L, H, m_pdf);
            brdfPdf += m_pdf * primarySpecRatio * (1.0 - diffuseRatio);

            // Clearcoat
            brdf += EvalClearcoat(material, V, N, L, H, m_pdf);
            brdfPdf += m_pdf * (1.0 - primarySpecRatio) * (1.0 - diffuseRatio);
        }

        pdf = lerp(brdfPdf, bsdfPdf, transWeight);

        return lerp(brdf, bsdf, transWeight);
    }

    public static float3 Sample(in Payload payload, in Material material, inout float3 L, inout float pdf)
    {
        pdf = 0.0;
        float3 f = float3(0.0, 0.0, 0.0);

        float diffuseRatio = 0.5 * (1.0 - material.Metallic);
        float transWeight = (1.0 - material.Metallic) * material.Transmission;

        float3 Cdlin = material.Albedo.xyz;
        float Cdlum = 0.3 * Cdlin.x + 0.6 * Cdlin.y + 0.1 * Cdlin.z; // luminance approx.

        float3 Ctint = Cdlum > 0.0 ? Cdlin / Cdlum : float3(1.0, 1.0, 1.0); // normalize lum. to isolate hue+sat
        float3 Cspec0 = lerp(material.Albedo.w * 0.08 * lerp(float3(1.0, 1.0, 1.0), Ctint, material.SpecularTint),
                             Cdlin, material.Metallic);
        float3 Csheen = lerp(float3(1.0, 1.0, 1.0), Ctint, material.SheenTint);
        float eta = payload.Eta;

        float3 N = payload.ForwardNormal;
        float3 V = -WorldRayDirection();

        float3x3 frame = Maths.LocalFrame(N);

        float r1 = Random.Rnd(payload.Seed);
        float r2 = Random.Rnd(payload.Seed);

        if (Random.Rnd(payload.Seed) < transWeight)
        {
            float3 H = Maths.ImportanceSampleGTR2(material.Roughness, r1, r2);
            H = mul(H, frame);

            if (dot(V, H) < 0.0)
                H = -H;

            float3 R = reflect(-V, H);
            float F = Maths.DielectricFresnel(abs(dot(R, H)), eta);

            // Reflection/Total internal reflection
            if (r2 < F)
            {
                L = normalize(R);
                f = EvalDielectricReflection(payload, material, V, N, L, H, pdf);
            }
            else // Transmission
            {
                L = normalize(refract(-V, H, eta));
                f = EvalDielectricRefraction(payload, material, V, N, L, H, pdf);
            }

            f *= transWeight;
            pdf *= transWeight;
        }
        else
        {
            if (Random.Rnd(payload.Seed) < diffuseRatio)
            {
                L = Maths.CosineSampleHemisphere(payload.Seed);
                L = mul(L, frame);

                float3 H = normalize(L + V);

                f = EvalDiffuse(material, Csheen, V, N, L, H, pdf);
                pdf *= diffuseRatio;
            }
            else // Specular
            {
                float primarySpecRatio = 1.0 / (1.0 + material.ClearCoat);

                // Sample primary specular lobe
                if (Random.Rnd(payload.Seed) < primarySpecRatio)
                {
                    // TODO: Implement http://jcgt.org/published/0007/04/01/
                    float3 H = Maths.ImportanceSampleGTR2(material.Roughness, r1, r2);
                    H = mul(H, frame);

                    if (dot(V, H) < 0.0)
                        H = -H;

                    L = normalize(reflect(-V, H));

                    f = EvalSpecular(material, Cspec0, V, N, L, H, pdf);
                    pdf *= primarySpecRatio * (1.0 - diffuseRatio);
                }
                else // Sample clearcoat lobe
                {
                    float3 H = Maths.ImportanceSampleGTR1(lerp(0.1, 0.001, material.ClearCoatGloss), r1, r2);
                    H = mul(H, frame);

                    if (dot(V, H) < 0.0)
                        H = -H;

                    L = normalize(reflect(-V, H));

                    f = EvalClearcoat(material, V, N, L, H, pdf);
                    pdf *= (1.0 - primarySpecRatio) * (1.0 - diffuseRatio);
                }
            }

            f *= (1.0 - transWeight);
            pdf *= (1.0 - transWeight);
        }

        return f;
    }

    private static float3 EvalDielectricReflection(in Payload payload, in Material material, in float3 V, in float3 N,
                                                   in float3 L, in float3 H, out float pdf)
    {
        pdf = 0.0;

        if (dot(N, L) <= 0.0)
        {
            return float3(0.0, 0.0, 0.0);
        }

        float eta = payload.Eta;

        float F = Maths.DielectricFresnel(dot(V, H), eta);
        float D = Maths.GTR2(dot(N, H), material.Roughness);

        pdf = D * dot(N, H) * F / (4.0 * abs(dot(V, H)));

        float G =
            Maths.SmithGGX(abs(dot(N, L)), material.Roughness) * Maths.SmithGGX(abs(dot(N, V)), material.Roughness);

        return material.Albedo.xyz * F * D * G;
    }

    private static float3 EvalDielectricRefraction(in Payload payload, in Material material, in float3 V, in float3 N,
                                                   in float3 L, in float3 H, out float pdf)
    {
        float eta = payload.Eta;

        pdf = 0.0;
        if (dot(N, L) >= 0.0)
        {
            return float3(0.0, 0.0, 0.0);
        }

        float F = Maths.DielectricFresnel(abs(dot(V, H)), eta);
        float D = Maths.GTR2(dot(N, H), material.Roughness);

        float denomSqrt = dot(L, H) + dot(V, H) * eta;
        pdf = D * dot(N, H) * (1.0 - F) * abs(dot(L, H)) / (denomSqrt * denomSqrt);

        float G =
            Maths.SmithGGX(abs(dot(N, L)), material.Roughness) * Maths.SmithGGX(abs(dot(N, V)), material.Roughness);

        return material.Albedo.xyz * (1.0 - F) * D * G * abs(dot(V, H)) * abs(dot(L, H)) * 4.0 * eta * eta /
               (denomSqrt * denomSqrt);
    }

    private static float3 EvalDiffuse(in Material material, in float3 Csheen, float3 V, float3 N, float3 L, float3 H,
                                      inout float pdf)
    {
        pdf = 0.0;
        if (dot(N, L) <= 0.0)
        {
            return float3(0.0, 0.0, 0.0);
        }

        pdf = dot(N, L) * (1.0 / Constants.PI);

        // Diffuse
        float FL = Maths.SchlickFresnel(dot(N, L));
        float FV = Maths.SchlickFresnel(dot(N, V));
        float FH = Maths.SchlickFresnel(dot(L, H));
        float Fd90 = 0.5 + 2.0 * dot(L, H) * dot(L, H) * material.Roughness;
        float Fd = lerp(1.0, Fd90, FL) * lerp(1.0, Fd90, FV);

        // Fake Subsurface TODO: Replace with volumetric scattering
        float Fss90 = dot(L, H) * dot(L, H) * material.Roughness;
        float Fss = lerp(1.0, Fss90, FL) * lerp(1.0, Fss90, FV);
        float ss = 1.25 * (Fss * (1.0 / (dot(N, L) + dot(N, V)) - 0.5) + 0.5);

        float3 Fsheen = FH * material.Sheen * Csheen;

        return ((1.0 / Constants.PI) * lerp(Fd, ss, material.SubSurface) * material.Albedo.xyz + Fsheen) *
               (1.0 - material.Metallic);
    }

    private static float3 EvalSpecular(in Material material, in float3 Cspec0, float3 V, float3 N, float3 L, float3 H,
                                       inout float pdf)
    {
        pdf = 0.0;
        if (dot(N, L) <= 0.0)
        {
            return float3(0.0, 0.0, 0.0);
        }

        float D = Maths.GTR2(dot(N, H), material.Roughness);
        pdf = D * dot(N, H) / (4.0 * dot(V, H));

        float FH = Maths.SchlickFresnel(dot(L, H));
        float3 F = lerp(Cspec0, float3(1.0), FH);
        float G =
            Maths.SmithGGX(abs(dot(N, L)), material.Roughness) * Maths.SmithGGX(abs(dot(N, V)), material.Roughness);

        return F * D * G;
    }

    private static float3 EvalClearcoat(in Material material, float3 V, float3 N, float3 L, float3 H, inout float pdf)
    {
        pdf = 0.0;
        if (dot(N, L) <= 0.0)
        {
            return float3(0.0, 0.0, 0.0);
        }

        float D = Maths.GTR1(dot(N, H), lerp(0.1, 0.001, material.ClearCoatGloss));
        pdf = D * dot(N, H) / (4.0 * dot(V, H));

        float FH = Maths.SchlickFresnel(dot(L, H));
        float F = lerp(0.04, 1.0, FH);
        float G = Maths.SmithGGX(dot(N, L), 0.25) * Maths.SmithGGX(dot(N, V), 0.25);

        return float3(0.25 * material.ClearCoat * F * D * G);
    }
}
