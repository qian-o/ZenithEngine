import "Common/Math";
import "Common/Random";
import "Common/Sampling";
import "Common/Structs";
import "Uniforms";

[shader("closesthit")]
void Main(inout Payload payload, BuiltInTriangleIntersectionAttributes attrib)
{
    Vertex vertex = Vertex.GetVertex(vertices, indices, offsets, attrib);
    Material material = materials[vertex.MaterialIndex];

    // Update the material properties using textures.

    // Albedo
    if (material.AlbedoTextureIndex >= 0)
    {
        texture2D texture = textures[material.AlbedoTextureIndex];
        SamplerState sampler = samplers[material.AlbedoTextureIndex];

        material.Albedo.rgb *= texture.Sample(sampler, vertex.TexCoord).rgb;
    }

    // Metallic and Roughness
    if (material.MetallicRoughnessTextureIndex >= 0)
    {
        texture2D texture = textures[material.MetallicRoughnessTextureIndex];
        SamplerState sampler = samplers[material.MetallicRoughnessTextureIndex];

        material.Metallic = texture.Sample(sampler, vertex.TexCoord).r;
        material.Roughness = texture.Sample(sampler, vertex.TexCoord).g;
    }

    // Normal Map
    if (material.NormalTextureIndex >= 0)
    {
        texture2D texture = textures[material.NormalTextureIndex];
        SamplerState sampler = samplers[material.NormalTextureIndex];

        vertex.Normal = normalize(texture.Sample(sampler, vertex.TexCoord).xyz * 2.0f - 1.0f);
    }

    payload.Position = vertex.Position;
    payload.Normal = vertex.Normal;
    payload.ForwardNormal = dot(vertex.Normal, WorldRayDirection()) > 0 ? -vertex.Normal : vertex.Normal;
    payload.Eta = dot(vertex.Normal, payload.ForwardNormal) > 0 ? 1.0f / material.IOR : material.IOR;

    LightSample lightSample;
    BSDFSample bsdfSample;

    payload.Radiance += material.Emission.rgb * payload.Beta;

    if (Sampling.InteresetsEmitter(lightSample, globals.LightCount, lights, RayTCurrent()))
    {
    }
}
