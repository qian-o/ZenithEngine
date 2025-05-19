import "Common/Structs";

uniform RaytracingAccelerationStructure scene;
uniform StructuredBuffer<Material> materials;
uniform StructuredBuffer<Vertex> vertices;
uniform StructuredBuffer<uint> indices;
uniform StructuredBuffer<uint2> offsets;
uniform Globals globals;
uniform RWTexture2D output;