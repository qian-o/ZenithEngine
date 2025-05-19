import "Common/Structs";

uniform RaytracingAccelerationStructure scene;
uniform StructuredBuffer<Vertex> vertexBuffers[4];
uniform StructuredBuffer<uint> indexBuffers[4];
uniform StructuredBuffer<Material> materials;
uniform Globals globals;
uniform RWTexture2D output;