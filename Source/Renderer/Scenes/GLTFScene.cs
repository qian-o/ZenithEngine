using System.Numerics;
using Graphics.Core;
using Graphics.Vulkan;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using StbiSharp;
using GLTFMaterial = SharpGLTF.Schema2.Material;
using GLTFNode = SharpGLTF.Schema2.Node;
using GLTFTexture = SharpGLTF.Schema2.Texture;
using Scene = Renderer.Components.Scene;
using Texture = Graphics.Vulkan.Texture;

namespace Renderer.Scenes;

internal sealed unsafe class GLTFScene(MainWindow mainWindow) : Scene(mainWindow)
{
    #region Structs
    private struct Vertex(Vector3 position, Vector3 normal, Vector2 texCoord, Vector3 color, Vector4 tangent)
    {
        public Vector3 Position = position;

        public Vector3 Normal = normal;

        public Vector2 TexCoord = texCoord;

        public Vector3 Color = color;

        public Vector4 Tangent = tangent;
    }

    private struct Primitive(uint firstIndex, uint indexCount, int materialIndex)
    {
        public uint FirstIndex = firstIndex;

        public uint IndexCount = indexCount;

        public int MaterialIndex = materialIndex;
    }
    #endregion

    #region Classes
    private sealed class Mesh
    {
        public List<Primitive> Primitives { get; } = [];
    }

    private sealed class Node
    {
        public string Name { get; set; } = string.Empty;

        public Node? Parent { get; set; }

        public List<Node> Children { get; } = [];

        public Mesh? Mesh { get; set; }

        public Matrix4x4 LocalTransform { get; set; } = Matrix4x4.Identity;

        public bool IsVisible { get; set; } = true;
    }

    private sealed class Material
    {
        public Vector4 BaseColorFactor { get; set; } = Vector4.One;

        public uint BaseColorTextureIndex { get; set; }

        public uint NormalTextureIndex { get; set; }

        public string AlphaMode { get; set; } = "OPAQUE";

        public float AlphaCutoff { get; set; } = 0.5f;

        public bool DoubleSided { get; set; }
    }
    #endregion

    private readonly List<Texture> _textures = [];
    private readonly List<Material> _materials = [];
    private readonly List<Node> _nodes = [];

    protected override void Initialize()
    {
        Title = "GLTF Scene";

        ModelRoot root = ModelRoot.Load("Assets/Models/Sponza/glTF/Sponza.gltf");

        foreach (GLTFTexture gltfTexture in root.LogicalTextures)
        {
            Stbi.InfoFromMemory(gltfTexture.PrimaryImage.Content.Content.Span, out int width, out int height, out _);
            StbiImage image = Stbi.LoadFromMemory(gltfTexture.PrimaryImage.Content.Content.Span, 4);

            TextureDescription description = TextureDescription.Texture2D((uint)width, (uint)height, 1, PixelFormat.R8G8B8A8UNorm, TextureUsage.Sampled);

            Texture texture = _resourceFactory.CreateTexture(in description);

            _graphicsDevice.UpdateTexture(texture, image.Data, 0, 0, 0, (uint)width, (uint)height, 1, 0, 0);

            _textures.Add(texture);

            image.Dispose();
        }

        foreach (GLTFMaterial gltfMaterial in root.LogicalMaterials)
        {
            Material material = new();

            if (gltfMaterial.FindChannel(KnownChannel.BaseColor.ToString()) is MaterialChannel baseColor)
            {
                material.BaseColorFactor = baseColor.Color;

                if (baseColor.Texture != null)
                {
                    material.BaseColorTextureIndex = (uint)baseColor.Texture.LogicalIndex;
                }
            }

            if (gltfMaterial.FindChannel(KnownChannel.Normal.ToString()) is MaterialChannel normal)
            {
                if (normal.Texture != null)
                {
                    material.NormalTextureIndex = (uint)normal.Texture.LogicalIndex;
                }
            }

            material.AlphaMode = gltfMaterial.Alpha.ToString();
            material.AlphaCutoff = gltfMaterial.AlphaCutoff;
            material.DoubleSided = gltfMaterial.DoubleSided;

            _materials.Add(material);
        }

        List<Vertex> vertices = [];
        List<uint> indices = [];

        foreach (GLTFNode gltfNode in root.LogicalNodes)
        {
            LoadNode(gltfNode, null);
        }

        void LoadNode(GLTFNode gltfNode, Node? parent)
        {
            Node node = new()
            {
                Name = gltfNode.Name,
                LocalTransform = gltfNode.LocalTransform.Matrix
            };

            foreach (GLTFNode children in gltfNode.VisualChildren)
            {
                LoadNode(children, node);
            }

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
                            Vector3 color = colorBuffer != null ? colorBuffer[(int)i] : Vector3.Zero;
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

                    node.Mesh ??= new();
                    node.Mesh.Primitives.Add(new Primitive(firsetIndex, (uint)indexCount, primitive.Material.LogicalIndex));
                }
            }

            if (parent != null)
            {
                node.Parent = parent;
                parent.Children.Add(node);
            }
            else
            {
                _nodes.Add(node);
            }
        }
    }

    protected override void RecreatePipeline(Framebuffer framebuffer)
    {
    }

    protected override void UpdateCore(UpdateEventArgs e)
    {
    }

    protected override void RenderCore(CommandList commandList, Framebuffer framebuffer, RenderEventArgs e)
    {
    }

    protected override void Destroy()
    {
        foreach (Texture texture in _textures)
        {
            texture.Dispose();
        }

        base.Destroy();
    }
}
