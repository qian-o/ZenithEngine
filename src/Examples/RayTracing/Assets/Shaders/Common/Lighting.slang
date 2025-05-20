import "Common/HDR";
import "Common/Structs";

public class Lighting
{
    public static float3 DirectLighting(in Payload payload, in Material material, uint lightCount, Light[] lights,
                                        bool useHdr, Texture2D[] hdrs)
    {
        float3 L = float3(0.0, 0.0, 0.0);

        BSDFSample bsdfSample;

        float3 surfacePos = payload.Position;

        if (useHdr)
        {
            float3 color = float3(0.0, 0.0, 0.0);
            // float4 dirPdf = HDR.EnvSample(color, );
        }

        return L;
    }
}
