import "HDR";
import "Structs";
import "Uniforms";

public class Lighting
{
    public static float3 DirectLighting(in Payload payload, in Material material)
    {
        float3 L = float3(0.0, 0.0, 0.0);

        BSDFSample bsdfSample;

        float3 surfacePos = payload.Position;

        if (globals.UseHdr)
        {
            float3 color = float3(0.0, 0.0, 0.0);
            // float4 dirPdf = HDR.EnvSample(color, );
        }
    }
}
