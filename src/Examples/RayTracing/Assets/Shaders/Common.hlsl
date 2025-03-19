const float C_Stack_Max = 3.402823466e+38;
const float EPS = 0.05;
const float M_PI = 3.141592653589;

uint CompressUnitVec(float3 nv)
{
    if ((nv.x < C_Stack_Max) && !isinf(nv.x))
    {
        const float d = 32767.0f / (abs(nv.x) + abs(nv.y) + abs(nv.z));
        
        int x = int(round(nv.x * d));
        int y = int(round(nv.y * d));
        
        if (nv.z < 0.0f)
        {
            const int maskx = x >> 31;
            const int masky = y >> 31;
            const int tmp = 32767 + maskx + masky;
            const int tmpx = x;
            x = (tmp - (y ^ masky)) ^ maskx;
            y = (tmp - (tmpx ^ maskx)) ^ masky;
        }
        
        uint packed = (uint(y + 32767) << 16) | uint(x + 32767);
        
        if (packed == ~0u)
        {
            return ~0x1u;
        }
        
        return packed;
    }
    else
    {
        return ~0u;
    }
}

float ShortToFloatM11(const int v)
{
    return (v >= 0) ? (asfloat(0x3F800000u | (uint(v) << 8)) - 1.0f) :
                      (asfloat((0x80000000u | 0x3F800000u) | (uint(-v) << 8)) + 1.0f);
}

float3 DecompressUnitVector(uint packed)
{
    if (packed != ~0u)
    {
        int x = int(packed & 0xFFFFu) - 32767;
        int y = int(packed >> 16) - 32767;
        
        const int maskx = x >> 31;
        const int masky = y >> 31;
        const int tmp0 = 32767 + maskx + masky;
        const int ymask = y ^ masky;
        const int tmp1 = tmp0 - (x ^ maskx);
        const int z = tmp1 - ymask;
        
        float zf;
        
        if (z < 0)
        {
            x = (tmp0 - ymask) ^ maskx;
            y = tmp1 ^ masky;
            zf = asfloat((0x80000000u | 0x3F800000u) | (uint(-z) << 8)) + 1.0f;
        }
        else
        {
            zf = asfloat(0x3F800000u | (uint(z) << 8)) - 1.0f;
        }
        
        return normalize(float3(ShortToFloatM11(x), ShortToFloatM11(y), zf));
    }
    else
    {
        return float3(C_Stack_Max, C_Stack_Max, C_Stack_Max);
    }
}

float3 OffsetRay(float3 p, float3 n)
{
    const float intScale = 256.0f;
    const float floatScale = 1.0f / 65536.0f;
    const float origin = 1.0f / 32.0f;
    
    int3 of_i = int3(intScale * n.x, intScale * n.y, intScale * n.z);
    
    float3 p_i = float3(asfloat(asint(p.x) + ((p.x < 0) ? -of_i.x : of_i.x)),
                        asfloat(asint(p.y) + ((p.y < 0) ? -of_i.y : of_i.y)),
                        asfloat(asint(p.z) + ((p.z < 0) ? -of_i.z : of_i.z)));
    
    return float3(abs(p.x) < origin ? p.x + floatScale * n.x : p_i.x,
                  abs(p.y) < origin ? p.y + floatScale * n.y : p_i.y,
                  abs(p.z) < origin ? p.z + floatScale * n.z : p_i.z);
}

void ComputeDefaultBasis(const float3 normal, out float3 x, out float3 y)
{
    float3 z = normal;
    const float yz = -z.y * z.z;
    
    y = normalize(((abs(z.z) > 0.99999f) ? float3(-z.x * z.y, 1.0f - z.y * z.y, yz) : float3(-z.x * z.z, yz, 1.0f - z.z * z.z)));
    x = cross(y, z);
}

uint tea(uint val0, uint val1)
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

uint2 pcg2d(uint2 v)
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

uint lcg(inout uint prev)
{
    uint LCG_A = 1664525u;
    uint LCG_C = 1013904223u;
    
    prev = (LCG_A * prev + LCG_C);
    
    return prev & 0x00FFFFFF;
}

float rnd(inout uint seed)
{
    return (float(lcg(seed)) / float(0x01000000));
}