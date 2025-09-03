struct Vertex
{
    float4 Position : SV_POSITION;

    float3 Normal : NORMAL;

    float2 TexCoord : TEXCOORD0;
}

struct Meshlet
{
    uint PrimitiveCount;

    uint Vertices[64];

    uint VertexCount;

    uint Indices[126];

    uint IndexCount;
};

uniform StructuredBuffer<Vertex> vertices;
uniform StructuredBuffer<Meshlet> meshlets;

[shader("mesh")]
[numthreads(1, 1, 1)]
[outputtopology("triangle")]
void MeshMain(in uint threadIndex: SV_GroupIndex, out vertices Vertex[64] outVertices, out indices uint[126] outIndices)
{
    Meshlet meshlet = meshlets[threadIndex];

    SetMeshOutputCounts(meshlet.VertexCount, meshlet.PrimitiveCount);

    for (uint i = 0; i < meshlet.VertexCount; i++)
    {
        outVertices[i] = vertices[meshlet.Vertices[i]];
    }

    for (uint i = 0; i < meshlet.IndexCount; i++)
    {
        outIndices[i] = meshlet.Indices[i];
    }
}

[shader("pixel")]
float4 PixelMain(Vertex input) : SV_TARGET
{
    return float4(input.Normal * 0.5 + 0.5, 1.0);
}
