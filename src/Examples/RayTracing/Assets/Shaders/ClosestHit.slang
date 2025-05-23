import "Common/Constants";
import "Common/DisneyBSDF";
import "Common/Lighting";
import "Common/Maths";
import "Common/Random";
import "Common/Sampling";
import "Common/Structs";

uniform ParameterBlock<Uniforms> uniforms;

[shader("closesthit")]
void Main(inout Payload payload, BuiltInTriangleIntersectionAttributes attrib)
{
    Vertex vertex = Vertex.GetVertex(uniforms, attrib);
    Material material = uniforms.Materials[vertex.MaterialIndex];

    // Update the material properties using textures.

    // Albedo
    if (material.AlbedoTextureIndex >= 0)
    {
        texture2D texture = uniforms.Textures[material.AlbedoTextureIndex];
        SamplerState sampler = uniforms.Samplers[material.AlbedoTextureIndex];

        material.Albedo.xyz *= texture.SampleLevel(sampler, vertex.TexCoord, 0).xyz;
    }

    // Metallic and Roughness
    if (material.MetallicRoughnessTextureIndex >= 0)
    {
        texture2D texture = uniforms.Textures[material.MetallicRoughnessTextureIndex];
        SamplerState sampler = uniforms.Samplers[material.MetallicRoughnessTextureIndex];

        material.Metallic = texture.SampleLevel(sampler, vertex.TexCoord, 0).r;
        material.Roughness = texture.SampleLevel(sampler, vertex.TexCoord, 0).g;
    }

    // Normal Map
    if (material.NormalTextureIndex >= 0)
    {
        texture2D texture = uniforms.Textures[material.NormalTextureIndex];
        SamplerState sampler = uniforms.Samplers[material.NormalTextureIndex];

        vertex.Normal = normalize(texture.SampleLevel(sampler, vertex.TexCoord, 0).xyz * 2.0f - 1.0f);
    }

    payload.Position = vertex.Position;
    payload.Normal = vertex.Normal;
    payload.ForwardNormal = dot(vertex.Normal, WorldRayDirection()) > 0 ? -vertex.Normal : vertex.Normal;
    payload.Eta = dot(vertex.Normal, payload.ForwardNormal) > 0 ? 1.0 / material.IOR : material.IOR;

    LightSample lightSample;
    BSDFSample bsdfSample;

    payload.Radiance += material.Emission.xyz * payload.Beta;

    if (Sampling.InteresetsEmitter(uniforms, RayTCurrent(), lightSample))
    {
        float3 Le = Sampling.SampleEmitter(payload, lightSample, payload.Bsdf);

        if (uniforms.Globals.DoubleSidedLighting || dot(payload.ForwardNormal, lightSample.Normal) > 0.0)
        {
            payload.Radiance += Le * payload.Beta;
        }

        payload.Stop = true;

        return;
    }

    payload.Beta *= exp(-payload.Absorbtion * RayTCurrent());
    payload.Radiance += Lighting.DirectLighting(uniforms, material, payload) * payload.Beta;

    float3 F = DisneyBSDF.Sample(payload, material, bsdfSample.Direction, bsdfSample.Pdf);

    float cosTheta = abs(dot(payload.ForwardNormal, bsdfSample.Direction));

    if (bsdfSample.Pdf <= 0.0)
    {
        payload.Stop = true;

        return;
    }

    payload.Beta *= F * cosTheta / (bsdfSample.Pdf + Constants.EPS);

    if (dot(payload.ForwardNormal, bsdfSample.Direction) < 0.0)
    {
        payload.Absorbtion = -log(material.Extinction.xyz) / (material.AttenuationDistance + Constants.EPS);
    }

    // Russian roulette
    if (Maths.Max3(payload.Beta) < 0.01 && payload.Depth > 2)
    {
        float q = max(float(0.05), 1.0 - Maths.Max3(payload.Beta));

        if (Random.Rnd(payload.Seed) < q)
        {
            payload.Stop = true;
        }

        payload.Beta /= (1.0 - q);
    }

    payload.Bsdf = bsdfSample;

    // Update a new ray path bounce direction
    payload.Ray.Direction = bsdfSample.Direction;
    payload.Ray.Origin = vertex.Position;
}
