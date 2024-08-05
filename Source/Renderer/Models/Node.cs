using System.Numerics;
using Renderer.Structs;
using SharpGLTF.Schema2;
using GltfNode = SharpGLTF.Schema2.Node;

namespace Renderer.Models;

internal sealed class Node
{
    public Node(GltfNode gltfNode, Node? parent, List<Vertex> vertices, List<uint> indices)
    {
        Name = gltfNode.Name;
        LocalTransform = gltfNode.LocalTransform.Matrix;
        Parent = parent;

        List<Node> children = [];
        foreach (GltfNode child in gltfNode.VisualChildren)
        {
            children.Add(new Node(child, this, vertices, indices));
        }
        Children = [.. children];

        List<Primitive> primitives = [];
        if (gltfNode.Mesh != null)
        {
            foreach (MeshPrimitive primitive in gltfNode.Mesh.Primitives)
            {
                uint firsetIndex = (uint)indices.Count;
                uint vertexOffset = (uint)vertices.Count;
                int indexCount = 0;

                // Vertices
                {
                    IList<Vector3>? positionBuffer = null;
                    IList<Vector3>? normalBuffer = null;
                    IList<Vector2>? texCoordBuffer = null;
                    IList<Vector3>? colorBuffer = null;
                    IList<Vector4>? tangentBuffer = null;
                    uint vertexCount = 0;

                    if (primitive.VertexAccessors.TryGetValue("POSITION", out Accessor? positionAccessor))
                    {
                        positionBuffer = positionAccessor.AsVector3Array();
                        vertexCount = (uint)positionAccessor.Count;
                    }

                    if (primitive.VertexAccessors.TryGetValue("NORMAL", out Accessor? normalAccessor))
                    {
                        normalBuffer = normalAccessor.AsVector3Array();
                    }

                    if (primitive.VertexAccessors.TryGetValue("TEXCOORD_0", out Accessor? texCoordAccessor))
                    {
                        texCoordBuffer = texCoordAccessor.AsVector2Array();
                    }

                    if (primitive.VertexAccessors.TryGetValue("COLOR_0", out Accessor? colorAccessor))
                    {
                        colorBuffer = colorAccessor.AsVector3Array();
                    }

                    if (primitive.VertexAccessors.TryGetValue("TANGENT", out Accessor? tangentAccessor))
                    {
                        tangentBuffer = tangentAccessor.AsVector4Array();
                    }

                    for (uint i = 0; i < vertexCount; i++)
                    {
                        Vector3 position = positionBuffer != null ? positionBuffer[(int)i] : Vector3.Zero;
                        Vector3 normal = normalBuffer != null ? normalBuffer[(int)i] : Vector3.Zero;
                        Vector2 texCoord = texCoordBuffer != null ? texCoordBuffer[(int)i] : Vector2.Zero;
                        Vector3 color = colorBuffer != null ? colorBuffer[(int)i] : Vector3.One;
                        Vector4 tangent = tangentBuffer != null ? tangentBuffer[(int)i] : Vector4.Zero;

                        vertices.Add(new Vertex(position, normal, texCoord, color, tangent));
                    }
                }

                // Indices
                {
                    if (primitive.IndexAccessor != null)
                    {
                        indexCount = primitive.IndexAccessor.Count;

                        IList<uint>? indexBuffer = primitive.IndexAccessor.AsIndicesArray();

                        for (int i = 0; i < indexCount; i++)
                        {
                            indices.Add(indexBuffer[i] + vertexOffset);
                        }
                    }
                }

                primitives.Add(new Primitive(firsetIndex, (uint)indexCount, primitive.Material.LogicalIndex));
            }
        }
        Primitives = [.. primitives];
    }

    public string Name { get; }

    public Matrix4x4 LocalTransform { get; }

    public Node? Parent { get; }

    public Node[] Children { get; }

    public Primitive[] Primitives { get; }
}
