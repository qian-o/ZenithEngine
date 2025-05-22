import "Maths";
import "Structs";

public class Sampling
{
    public static bool InteresetsEmitter(in Uniforms uniforms, float hit, inout LightSample lightSample)
    {
        float closest = FAR;

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

        return closest < FAR;
    }

    public static float3 SampleEmitter(in Payload payload, in LightSample lightSample, in BSDFSample bsdfSample)
    {
        float3 Le = lightSample.Emission;

        return payload.Depth == 0 ? Le : Maths.PowerHeuristic(bsdfSample.Pdf, lightSample.Pdf) * Le;
    }

    private static void CheckAreaLightIntersection(in Light light, float hit, inout float closest,
                                                   inout LightSample lightSample)
    {
        float dist = PlaneIntersect(light);

        if (dist < 0.0)
        {
            dist = FAR;
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
                                                     inout LightSample lightSample)
    {
        float dist = SphereIntersect(light);

        if (dist < 0.0)
        {
            dist = FAR;
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

        if (t > EPS)
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

        return FAR;
    }

    private static float SphereIntersect(in Light light)
    {
        float3 dir = light.Position - WorldRayOrigin();
        float b = dot(dir, WorldRayDirection());
        float det = b * b - dot(dir, dir) + light.Radius * light.Radius;

        if (det < 0.0)
        {
            return FAR;
        }

        det = sqrt(det);

        float t1 = b - det;

        if (t1 > EPS)
        {
            return t1;
        }

        float t2 = b + det;

        if (t2 > EPS)
        {
            return t2;
        }

        return FAR;
    }
}
