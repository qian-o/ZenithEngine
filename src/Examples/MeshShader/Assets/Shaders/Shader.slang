const static uint MAX_PRIMITIVES = 126;

struct Vertex
{
    float4 Position : SV_POSITION;

    float3 Normal : NORMAL;

    float2 TexCoord : TEXCOORD0;
}

struct Meshlet
{
    uint VertexOffset;

    uint VertexCount;

    uint PrimitiveOffset;

    uint PrimitiveCount;
};

uniform StructuredBuffer<Vertex> vertices;
uniform StructuredBuffer<uint3> indices;
uniform StructuredBuffer<Meshlet> meshlets;

[shader("mesh")]
[numthreads(MAX_PRIMITIVES, 1, 1)]
[outputtopology("triangle")]
void MeshMain(in uint groupId: SV_GroupID, in uint groupThreadId: SV_GroupThreadID,
              OutputVertices<Vertex, MAX_PRIMITIVES> outVertices, OutputIndices<uint3, MAX_PRIMITIVES> outIndices)
{
    Meshlet meshlet = meshlets[groupId];

    if (groupThreadId >= meshlet.VertexCount && groupThreadId >= meshlet.PrimitiveCount)
    {
        return;
    }

    SetMeshOutputCounts(meshlet.VertexCount, meshlet.PrimitiveCount);

    if (groupThreadId < meshlet.VertexCount)
    {
        outVertices[groupThreadId] = vertices[meshlet.VertexOffset + groupThreadId];
    }

    if (groupThreadId < meshlet.PrimitiveCount)
    {
        outIndices[groupThreadId] = indices[meshlet.PrimitiveOffset + groupThreadId];
    }
}

[shader("pixel")]
float4 PixelMain(Vertex vertex) : SV_TARGET
{
    return float4(vertex.TexCoord, 1.0);
}
