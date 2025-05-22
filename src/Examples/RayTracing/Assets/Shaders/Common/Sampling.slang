import "Constants";
import "Maths";
import "Random";
import "Structs";

public class Sampling
{
    public static bool InteresetsEmitter(in Uniforms uniforms, float hit, out LightSample lightSample)
    {
        float closest = Constants.FAR;

        uint lightCount = uniforms.Lights.getCount();

        for (uint i = 0; i < lightCount; i++)
        {
            Light light = uniforms.Lights[i];

            if (light.Type == LightType.Area)
            {
                CheckAreaLightIntersection(light, hit, closest, lightSample);
            }
            else
            {
                CheckSphereLightIntersection(light, hit, closest, lightSample);
            }
        }

        return closest < Constants.FAR;
    }

    public static float3 SampleEmitter(in Payload payload, in LightSample lightSample, in BSDFSample bsdfSample)
    {
        float3 Le = lightSample.Emission;

        return payload.Depth == 0 ? Le : Maths.PowerHeuristic(bsdfSample.Pdf, lightSample.Pdf) * Le;
    }

    public static LightSample SampleLight(in Uniforms uniforms, in Light light, inout uint seed)
    {
        return light.Type == LightType.Area ? SampleAreaLight(uniforms, light, seed)
                                            : SampleSphereLight(uniforms, light, seed);
    }

    private static void CheckAreaLightIntersection(in Light light, float hit, inout float closest,
                                                   out LightSample lightSample)
    {
        float dist = PlaneIntersect(light);

        if (dist < 0.0)
        {
            dist = Constants.FAR;
        }

        if (dist < closest && dist < hit)
        {
            closest = dist;

            float3 normal = normalize(cross(light.U, light.V));
            float cosTheta = abs(dot(-WorldRayDirection(), normal));
            float pdf = (dist * dist) / (cosTheta * light.Area);

            lightSample.Emission = light.Emission;
            lightSample.Pdf = pdf;
            lightSample.Normal = normal;
        }
    }

    private static void CheckSphereLightIntersection(in Light light, float hit, inout float closest,
                                                     out LightSample lightSample)
    {
        float dist = SphereIntersect(light);

        if (dist < 0.0)
        {
            dist = Constants.FAR;
        }

        if (dist < closest && dist < hit)
        {
            closest = dist;

            float3 surfacePos = WorldRayOrigin() + hit * WorldRayDirection();
            float3 normal = normalize(surfacePos - light.Position);
            float pdf = (dist * dist) / light.Area;

            lightSample.Emission = light.Emission;
            lightSample.Pdf = pdf;
            lightSample.Normal = normal;
        }
    }

    private static float PlaneIntersect(in Light light)
    {
        float3 u = light.U;
        float3 v = light.V;

        float3 normal = normalize(cross(u, v));
        float4 plane = float4(normal, dot(normal, light.Position));

        u *= 1.0 / dot(u, u);
        v *= 1.0 / dot(v, v);

        float3 n = plane.xyz;
        float dt = dot(WorldRayDirection(), n);
        float t = (plane.w - dot(n, WorldRayOrigin())) / dt;
        float3 pos = WorldRayOrigin() + t * WorldRayDirection();
        float3 view = pos - light.Position;

        if (t > Constants.EPS)
        {
            float a1 = dot(u, view);
            if (a1 >= 0.0 && a1 <= 1.0)
            {
                float a2 = dot(v, view);
                if (a2 >= 0.0 && a2 <= 1.0)
                {
                    return t;
                }
            }
        }

        return Constants.FAR;
    }

    private static float SphereIntersect(in Light light)
    {
        float3 dir = light.Position - WorldRayOrigin();
        float b = dot(dir, WorldRayDirection());
        float det = b * b - dot(dir, dir) + light.Radius * light.Radius;

        if (det < 0.0)
        {
            return Constants.FAR;
        }

        det = sqrt(det);

        float t1 = b - det;

        if (t1 > Constants.EPS)
        {
            return t1;
        }

        float t2 = b + det;

        if (t2 > Constants.EPS)
        {
            return t2;
        }

        return Constants.FAR;
    }

    private static LightSample SampleAreaLight(in Uniforms uniforms, in Light light, inout uint seed)
    {
        float r1 = Random.Rnd(seed);
        float r2 = Random.Rnd(seed);

        float3 u = light.U * r1;
        float3 v = light.V * r2;

        LightSample lightSample;
        lightSample.Normal = normalize(cross(light.U, light.V));
        lightSample.Emission = light.Emission * float(uniforms.Lights.getCount());
        lightSample.Position = light.Position + u + v;

        return lightSample;
    }

    private static LightSample SampleSphereLight(in Uniforms uniforms, in Light light, inout uint seed)
    {
        float3 position = light.Position + UniformSampleSphere(seed) * light.Radius;

        LightSample lightSample;
        lightSample.Normal = normalize(position - light.Position);
        lightSample.Emission = light.Emission * float(uniforms.Lights.getCount());
        lightSample.Position = position;

        return lightSample;
    }

    private static float3 UniformSampleSphere(inout uint seed)
    {
        float r1 = Random.Rnd(seed);
        float r2 = Random.Rnd(seed);

        float z = 1.0 - 2.0 * r1;
        float r = sqrt(max(0.0, 1.0 - z * z));
        float phi = 2.0 * Constants.PI * r2;

        float x = r * cos(phi);
        float y = r * sin(phi);

        return float3(x, y, z);
    }
}
