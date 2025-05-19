import "Common/Structs";

// Space 0
uniform RaytracingAccelerationStructure scene;
uniform StructuredBuffer<Material> materials;
uniform StructuredBuffer<Vertex> vertices;
uniform StructuredBuffer<uint> indices;
uniform StructuredBuffer<uint2> offsets;
uniform Globals globals;
uniform RWTexture2D accumulation;
uniform RWTexture2D output;

// Space 1
uniform Texture2D textures[];

// Space 2
uniform SamplerState samplers[];