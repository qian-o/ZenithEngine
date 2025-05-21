import "Structs";

uniform RaytracingAccelerationStructure scene;

uniform StructuredBuffer<Material> materials;

uniform StructuredBuffer<Vertex> vertices;
uniform StructuredBuffer<uint> indices;
uniform StructuredBuffer<uint2> offsets;

uniform Globals globals;

uniform Texture2D textures[128];
uniform SamplerState samplers[128];

uniform Texture2D hdrTextures[3];
uniform SamplerState hdrSamplers[3];

uniform StructuredBuffer<Light> lights;

uniform RWTexture2D accumulation;
uniform RWTexture2D output;
