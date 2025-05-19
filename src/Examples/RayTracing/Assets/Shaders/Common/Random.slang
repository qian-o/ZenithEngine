public class Random
{
    public static float3 OffsetRay(float3 p, float3 n)
    {
        const float intScale = 256.0f;
        const float floatScale = 1.0f / 65536.0f;
        const float origin = 1.0f / 32.0f;

        int3 of_i = int3(float3(intScale * n.x, intScale * n.y, intScale * n.z));

        float3 p_i = float3(asfloat(asint(p.x) + ((p.x < 0) ? -of_i.x : of_i.x)),
                            asfloat(asint(p.y) + ((p.y < 0) ? -of_i.y : of_i.y)),
                            asfloat(asint(p.z) + ((p.z < 0) ? -of_i.z : of_i.z)));

        return float3(abs(p.x) < origin ? p.x + floatScale * n.x : p_i.x,
                      abs(p.y) < origin ? p.y + floatScale * n.y : p_i.y,
                      abs(p.z) < origin ? p.z + floatScale * n.z : p_i.z);
    }

    public static void ComputeDefaultBasis(const float3 normal, out float3 x, out float3 y)
    {
        float3 z = normal;
        const float yz = -z.y * z.z;

        y = normalize(((abs(z.z) > 0.99999f) ? float3(-z.x * z.y, 1.0f - z.y * z.y, yz)
                                             : float3(-z.x * z.z, yz, 1.0f - z.z * z.z)));
        x = cross(y, z);
    }

    public static uint Tea(uint val0, uint val1)
    {
        uint v0 = val0;
        uint v1 = val1;
        uint s0 = 0;

        for (uint n = 0; n < 16; n++)
        {
            s0 += 0x9e3779b9;
            v0 += ((v1 << 4) + 0xa341316c) ^ (v1 + s0) ^ ((v1 >> 5) + 0xc8013ea4);
            v1 += ((v0 << 4) + 0xad90777d) ^ (v0 + s0) ^ ((v0 >> 5) + 0x7e95761e);
        }

        return v0;
    }

    public static uint2 Pcg2d(uint2 v)
    {
        v = v * 1664525u + 1013904223u;

        v.x += v.y * 1664525u;
        v.y += v.x * 1664525u;

        v = v ^ (v >> 16u);

        v.x += v.y * 1664525u;
        v.y += v.x * 1664525u;

        v = v ^ (v >> 16u);

        return v;
    }

    public static uint Lcg(inout uint prev)
    {
        uint LCG_A = 1664525u;
        uint LCG_C = 1013904223u;

        prev = (LCG_A * prev + LCG_C);

        return prev & 0x00FFFFFF;
    }

    public static float Rnd(inout uint seed)
    {
        return (float(Lcg(seed)) / float(0x01000000));
    }
}
